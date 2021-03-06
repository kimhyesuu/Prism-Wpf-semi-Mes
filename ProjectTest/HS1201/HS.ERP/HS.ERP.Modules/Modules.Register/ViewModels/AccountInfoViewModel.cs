﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using HS.ERP.Business.Models;
using HS.ERP.Business.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace HS.ERP.Outlook.Core.Dialogs.ViewModels
{
   public class AccountInfoViewModel : BindableBase, IDialogAware
   {
      private ObservableCollection<Account> _accountList;
      private Account _accountInfo;
      private Account _selectedAccountInfo;

      #region 프로퍼티
      private IServiceLogic<Account> ServiceLogic { get; }

      public ObservableCollection<Account> AccountList
      {
         get { return _accountList; }
         set { SetProperty(ref _accountList, value); }
      }

      public Account AccountInformation
      {
         get { return _accountInfo; }
         set { SetProperty(ref _accountInfo, value); }
      }

      public Account SelectedAccountInfo
      {
         get { return _selectedAccountInfo; }
         set { SetProperty(ref _selectedAccountInfo, value); }
      }

      public string SelectedCompanyHeadNumber
      {
         get { return AccountInformation.CompanyPhoneNumber[0]; }
         set { SetProperty(ref AccountInformation.CompanyPhoneNumber[0], value); }
      }

      public string SelectedContactHeadNumber
      {
         get { return AccountInformation.ContactPhoneNumber[0]; }
         set { SetProperty(ref AccountInformation.ContactPhoneNumber[0], value); }
      }
      #endregion

      public AccountInfoViewModel()
      {
         ServiceLogic = new AccountService();
         DataInitialize();
         CommandInitialize();
      }

      private void CommandInitialize()
      {
         CloseDialogCommand = new DelegateCommand<string>(CloseDialog);
         SaveAccountInfoCommand = new DelegateCommand(SaveAccountInfoInStorage);
         DeleteAccountInfoCommand = new DelegateCommand<object>(o => DeleteAccountInfoInStorage(o));
         UpdateAccountCommand = new DelegateCommand(UpdateAccountInfoInStorage);
      }

      private void DataInitialize()
      {
         SelectedAccountInfo = new Account();
         AccountInformation = new Account();

         var accountList = ServiceLogic.GetAll();

         if (accountList != null && accountList.Any())
         {
            AccountList = new ObservableCollection<Account>(accountList);
            return;
         }

         AccountList = new ObservableCollection<Account>();
      }

      public DelegateCommand<object> DeleteAccountInfoCommand { get; private set; }
      public DelegateCommand SaveAccountInfoCommand { get; private set; }
      public DelegateCommand UpdateAccountCommand { get; private set; }
      public DelegateCommand<string> CloseDialogCommand { get; private set; }

      private void DeleteAccountInfoInStorage(object selectedList)
      {
         if (selectedList is null) return;

         var selectedAccount = selectedList as Account;
      
         ServiceLogic.Delete(selectedAccount.AccountId);

         if (AccountList.Count() > 0)
            AccountList.Remove(selectedAccount);

         MessageBox.Show($"{selectedAccount.CompanyName}이 삭제되었습니다.");          
      }

      private void UpdateAccountInfoInStorage()
      {
         var AccountToBeModified = SelectedAccountInfo;
         AccountInformation = AccountToBeModified;        
      }

      #region 거래처 정보를 저장하는 로직 
      private void SaveAccountInfoInStorage()
      {
         var companyNumber = SelectedCompanyHeadNumber;
         var contactNumber = GetPhoneHeadNumber(SelectedContactHeadNumber);

         if (AccountInformation.AccountId != null)
         {
            ServiceLogic.Update(SelectedAccountInfo);
            // 여기서 업데이트를 할 것입니다.....
            // 아이디 값이 없으면 
            //AccountList.Add(ServiceLogic.Update(AccountInformation));
            return;
         }

         if (IsCompatibility(companyNumber, contactNumber) != true)
         {
            MessageBox.Show("값을 넣어주세요");
            return;
         }


         ShowAddedAccount(ServiceLogic.Insert(AccountInformation));

         MessageBox.Show("성공");
      }

      private void ShowAddedAccount(Account account)
      {
         AccountList.Add(new Account
         {
            AccountId = account.AccountId,
            CompanyName = account.CompanyName,
            CompanyPhoneNumber = account.CompanyPhoneNumber,
            CompanyEmail = account.CompanyEmail,
            Address = account.Address,
            Description = account.Description,
            CreatedDate = account.CreatedDate,
            UpdatedDate = account.UpdatedDate,
            ContactId = account.ContactId,
            ContactName = account.ContactName,
            Department = account.Department,
            Position = account.Position,
            ContactPhoneNumber = account.ContactPhoneNumber
         });
      }

      private bool IsCompatibility(string companyNumber, string contactNumber)
      {
         var rd = new Random();
         var accountInfo = AccountInformation;
         var phoneHeadNumber = accountInfo.CompanyPhoneNumber[0];
         var contactHeadNumber = accountInfo.ContactPhoneNumber[0];

         accountInfo.AccountId = int.Parse(DateTime.Now.ToString("yyyyMMdd") + rd.Next(1, 100));
         accountInfo.ContactId = accountInfo.AccountId;

         phoneHeadNumber = phoneHeadNumber is null ? "010" : GetLocalHeadNumber(phoneHeadNumber);
         contactHeadNumber = contactHeadNumber is null ? "010" : GetPhoneHeadNumber(contactHeadNumber);

         if (!(accountInfo.AccountId != null
            && accountInfo.CompanyName != null
            && accountInfo.CompanyEmail != null
            && accountInfo.CompanyPhoneNumber[1] != null
            && accountInfo.CompanyPhoneNumber[2] != null
            && accountInfo.Address != null
            && accountInfo.ContactId != null
            && accountInfo.ContactName != null
            && accountInfo.CompanyPhoneNumber[1] != null
            && accountInfo.CompanyPhoneNumber[2] != null
            )) return false;

         return true;
      }

      public string GetLocalHeadNumber(string selectedCompanyPhoneNumber)
      {
         switch (selectedCompanyPhoneNumber)
         {
            case "Seoul":
               {
                  selectedCompanyPhoneNumber = "02";
                  break;
               }
            case "Incheon":
               {
                  selectedCompanyPhoneNumber = "032";
                  break;
               }
            case "Daejeon":
               {
                  selectedCompanyPhoneNumber = "042";
                  break;
               }
            case "Gyeonggi":
               {
                  selectedCompanyPhoneNumber = "031";
                  break;
               }
            case "Busan":
               {
                  selectedCompanyPhoneNumber = "051";
                  break;
               }
            default:
               {
                  selectedCompanyPhoneNumber = null;
                  break;
               }
         }
         return selectedCompanyPhoneNumber;
      }

      public string GetPhoneHeadNumber(string phoneHeadNumber)
      {
         switch (phoneHeadNumber)
         {
            case "ZOZ":
               {
                  phoneHeadNumber = "010";
                  break;
               }
            case "ZOO":
               {
                  phoneHeadNumber = "011";
                  break;
               }
            case "ZOS":
               {
                  phoneHeadNumber = "017";
                  break;
               }
            case "ZON":
               {
                  phoneHeadNumber = "019";
                  break;
               }
            default:
               {
                  phoneHeadNumber = null;
                  break;
               }
         }
         return phoneHeadNumber;
      }


      #endregion

      //collection값을 넣어주면 되겠네
      #region 다이얼로그 리절트를 받기 위해 쓰이는 기술
      public event Action<IDialogResult> RequestClose;

      private void CloseDialog(string parameter)
      {
         ButtonResult result = ButtonResult.None;
         var transportParameter = new DialogParameters();
         string parameterValue = string.Empty;

         if (parameter?.ToLower() == "true")
         {
            result = ButtonResult.OK;
            parameterValue = "ButtonResult.OK";
         }

         transportParameter.Add("submessage", parameterValue);
         RaiseRequestClose(result, transportParameter);
      }

      private void RaiseRequestClose(ButtonResult dialogResult, DialogParameters dialogParameters)
        => RequestClose?.Invoke(new DialogResult(dialogResult, dialogParameters));

      public bool CanCloseDialog()
      {
         return true;
      }

      public void OnDialogClosed()
      {

      }

      public void OnDialogOpened(IDialogParameters parameters)
      {
         //Message = parameters.GetValue<string>("message");
      }

      #endregion

      #region TitleName

      private string _message;
      public string Message
      {
         get => _message;
         set { SetProperty(ref _message, value); }
      }

      public string Title => "Account";

      #endregion
   }


}
