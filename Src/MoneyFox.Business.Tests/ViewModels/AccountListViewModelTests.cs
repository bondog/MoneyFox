﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyFox.Business.Manager;
using MoneyFox.Business.ViewModels;
using MoneyFox.DataAccess.DataServices;
using MoneyFox.DataAccess.Pocos;
using MoneyFox.Foundation.Interfaces;
using Moq;
using MvvmCross.Navigation;
using MvvmCross.Tests;
using Should;
using Xunit;

namespace MoneyFox.Business.Tests.ViewModels
{
    [Collection("MvxIocCollection")]
    public class AccountListViewModelTests : MvxIoCSupportingTest
    {
        private readonly Mock<IAccountService> accountServiceMock;

        public AccountListViewModelTests()
        {
            accountServiceMock = new Mock<IAccountService>();
            accountServiceMock.SetupAllProperties();
        }

        [Fact]
        public void DeleteAccountCommand_UserReturnTrue_ExecuteDeletion()
        {
            // Arrange
            var deleteCalled = false;

            accountServiceMock.Setup(x => x.DeleteAccount(It.IsAny<Account>()))
                              .Callback(() => deleteCalled = true)
                              .Returns(Task.CompletedTask);
            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            var dialogServiceSetup = new Mock<IDialogService>();
            dialogServiceSetup.Setup(x => x.ShowConfirmMessage(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Returns(Task.FromResult(true));

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                dialogServiceSetup.Object, 
                new Mock<IMvxNavigationService>().Object);

            // Act
            viewModel.DeleteAccountCommand.Execute(new AccountViewModel(new Account {Data = {Id = 3}}));

            // Assert
            deleteCalled.ShouldBeTrue();
        }

        [Fact]
        public void DeleteAccountCommand_UserReturnFalse_SkipDeletion()
        {
            // Arrange
            var deleteCalled = false;
            accountServiceMock.Setup(x => x.DeleteAccount(It.IsAny<Account>())).Callback(() => deleteCalled = true);
            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            var dialogServiceSetup = new Mock<IDialogService>();
            dialogServiceSetup.Setup(x => x.ShowConfirmMessage(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Returns(Task.FromResult(false));

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                dialogServiceSetup.Object,
                new Mock<IMvxNavigationService>().Object);

            // Act
            viewModel.DeleteAccountCommand.Execute(new AccountViewModel(new Account {Data = {Id = 3}}));

            // Assert
            deleteCalled.ShouldBeFalse();
        }

        [Fact]
        public void DeleteAccountCommand_AccountNull_DoNothing()
        {
            // Arrange
            var deleteCalled = false;

            accountServiceMock.Setup(x => x.DeleteAccount(It.IsAny<Account>())).Callback(() => deleteCalled = true);
            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            var dialogServiceSetup = new Mock<IDialogService>();
            dialogServiceSetup.Setup(x => x.ShowConfirmMessage(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Returns(Task.FromResult(true));

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                dialogServiceSetup.Object, 
                new Mock<IMvxNavigationService>().Object);

            // Act
            viewModel.DeleteAccountCommand.Execute(null);

            // Assert
            deleteCalled.ShouldBeFalse();
        }

        [Fact]
        public async void IsAllAccountsEmpty_AccountsEmpty_True()
        {
            // Arrange
            accountServiceMock.Setup(x => x.GetExcludedAccounts()).ReturnsAsync(new List<Account>());
            accountServiceMock.Setup(x => x.GetNotExcludedAccounts()).ReturnsAsync(new List<Account>());

            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IDialogService>().Object, 
                new Mock<IMvxNavigationService>().Object);

            // Act
            await viewModel.Initialize();

            // Assert
            viewModel.HasNoAccounts.ShouldBeTrue();
        }

        [Fact]
        public void IsAllAccountsEmpty_OneAccountInExcluded_False()
        {
            // Arrange
            accountServiceMock.Setup(x => x.GetNotExcludedAccounts()).ReturnsAsync(new List<Account>());
            accountServiceMock.Setup(x => x.GetExcludedAccounts())
                .ReturnsAsync(new List<Account> { new Account() });

            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IDialogService>().Object, 
                new Mock<IMvxNavigationService>().Object);

            // Act
            viewModel.ViewAppearing();

            // Assert
            viewModel.HasNoAccounts.ShouldBeFalse();
        }

        [Fact]
        public void IsAllAccountsEmpty_OneAccountInNotExcluded_False()
        {
            // Arrange
            accountServiceMock.Setup(x => x.GetExcludedAccounts()).ReturnsAsync(new List<Account>());
            accountServiceMock.Setup(x => x.GetNotExcludedAccounts())
                .ReturnsAsync(new List<Account> { new Account() });

            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IDialogService>().Object, 
                new Mock<IMvxNavigationService>().Object);

            // Act
            viewModel.ViewAppearing();

            // Assert
            viewModel.HasNoAccounts.ShouldBeFalse();
        }

        [Fact]
        public void IsAllAccountsEmpty_TwoAccountNotExcluded_False()
        {
            // Arrange
            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            accountServiceMock.Setup(x => x.GetExcludedAccounts()).ReturnsAsync(new List<Account>());
            accountServiceMock.Setup(x => x.GetNotExcludedAccounts())
                .ReturnsAsync(new List<Account>
                {
                    new Account(),
                    new Account()
                });

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IDialogService>().Object, 
                new Mock<IMvxNavigationService>().Object);

            // Act
            viewModel.ViewAppearing();

            // Assert
            viewModel.HasNoAccounts.ShouldBeFalse();
        }

        [Fact]
        public void IsAllAccountsEmpty_TwoAccountExcluded_False()
        {
            // Arrange
            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            accountServiceMock.Setup(x => x.GetNotExcludedAccounts()).ReturnsAsync(new List<Account>());
            accountServiceMock.Setup(x => x.GetExcludedAccounts())
                .ReturnsAsync(new List<Account>
                {
                    new Account(),
                    new Account()
                });

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IDialogService>().Object, 
                new Mock<IMvxNavigationService>().Object);

            // Act
            viewModel.ViewAppearing();

            // Assert
            viewModel.HasNoAccounts.ShouldBeFalse();
        }

        [Fact]
        public void IncludedAccounts_AccountsAvailable_MatchesRepository()
        {
            // Arrange
            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            accountServiceMock.Setup(x => x.GetExcludedAccounts()).ReturnsAsync(new List<Account>());
            accountServiceMock.Setup(x => x.GetNotExcludedAccounts())
                .ReturnsAsync(new List<Account>
                {
                    new Account { Data = {Id = 22}},
                    new Account {Data = {Id = 33}}
                });

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IDialogService>().Object, 
                new Mock<IMvxNavigationService>().Object);

            // Act
            viewModel.ViewAppearing();

            // Assert
            viewModel.Accounts.Count.ShouldEqual(1);
            viewModel.Accounts[0].Count.ShouldEqual(2);
            viewModel.Accounts[0][0].Id.ShouldEqual(22);
            viewModel.Accounts[0][1].Id.ShouldEqual(33);
        }

        [Fact]
        public void ExcludedAccounts_AccountsAvailable_MatchesRepository()
        {
            // Arrange
            var balanceCalculationManager = new Mock<IBalanceCalculationManager>();

            accountServiceMock.Setup(x => x.GetNotExcludedAccounts()).ReturnsAsync(new List<Account>());
            accountServiceMock.Setup(x => x.GetExcludedAccounts())
                .ReturnsAsync(new List<Account>
                {
                    new Account { Data = {Id = 22}},
                    new Account {Data = {Id = 33}}
                });

            var viewModel = new AccountListViewModel(accountServiceMock.Object,
                balanceCalculationManager.Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IDialogService>().Object, 
                new Mock<IMvxNavigationService>().Object);

            // Act
            viewModel.ViewAppearing();

            // Assert
            viewModel.Accounts.Count.ShouldEqual(1);
            viewModel.Accounts[0].Count.ShouldEqual(2);
            viewModel.Accounts[0][0].Id.ShouldEqual(22);
            viewModel.Accounts[0][1].Id.ShouldEqual(33);
        }
    }
}