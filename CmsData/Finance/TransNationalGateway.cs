﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CmsData.Finance.TransNational.Core;
using CmsData.Finance.TransNational.Query;
using CmsData.Finance.TransNational.Transaction.Refund;
using CmsData.Finance.TransNational.Transaction.Sale;
using CmsData.Finance.TransNational.Transaction.Void;
using CmsData.Finance.TransNational.Vault;
using UtilityExtensions;

namespace CmsData.Finance
{
    internal class TransNationalGateway : IGateway
    {
        private readonly string _userName;
        private readonly string _password;
        private CMSDataContext db;

        public string GatewayType
        {
            get { return "TransNational"; }
        }

        public TransNationalGateway(CMSDataContext db, bool testing)
        {
            this.db = db;

            if(testing || db.Setting("GatewayTesting", "false").ToLower() == "true")
            {
                _userName = "faithbased";
                _password = "bprogram2";
            }
            else
            {
                _userName = db.Setting("TNBUsername", "");
                _password = db.Setting("TNBPassword", "");

                if (string.IsNullOrWhiteSpace(_userName))
                    throw new Exception("TNBUsername setting not found, which is required for TransNational.");
                if (string.IsNullOrWhiteSpace(_password))
                    throw new Exception("TNBPassword setting not found, which is required for TransNational.");
            }
        }

        public void StoreInVault(int peopleId, string type, string cardNumber, string expires, string cardCode,
            string routing, string account, bool giving)
        {
            var person = db.LoadPersonById(peopleId);
            var paymentInfo = person.PaymentInfo();
            if (paymentInfo == null)
            {
                paymentInfo = new PaymentInfo();
                person.PaymentInfos.Add(paymentInfo);
            }

            if (type == PaymentType.CreditCard)
            {
                if (paymentInfo.TbnCardVaultId == null) // create new vault.
                    paymentInfo.TbnCardVaultId = CreateCreditCardVault(person, cardNumber, expires);
                else
                {
                    // update existing vault.
                    // check for updating the entire card or only expiration.
                    if (!cardNumber.StartsWith("X"))
                        UpdateCreditCardVault(paymentInfo.TbnCardVaultId.GetValueOrDefault(), person, cardNumber,
                            expires);
                    else
                        UpdateCreditCardVault(paymentInfo.TbnCardVaultId.GetValueOrDefault(), person, expires);
                }

                paymentInfo.MaskedCard = Util.MaskCC(cardNumber);
                paymentInfo.Ccv = cardCode; // TODO: shouldn't need to store this
                paymentInfo.Expires = expires;
            }
            else if (type == PaymentType.Ach)
            {
                if (paymentInfo.TbnBankVaultId == null) // create new vault
                    paymentInfo.TbnBankVaultId = CreateAchVault(person, account, routing);
                else
                {
                    // we can only update the ach account if there is a full account number.
                    if (!account.StartsWith("X"))
                        UpdateAchVault(paymentInfo.TbnBankVaultId.GetValueOrDefault(), person, account, routing);
                    else
                        UpdateAchVault(paymentInfo.TbnBankVaultId.GetValueOrDefault(), person);
                }

                paymentInfo.MaskedAccount = Util.MaskAccount(account);
                paymentInfo.Routing = Util.Mask(new StringBuilder(routing), 2);
            }
            else
                throw new ArgumentException("Type {0} not supported".Fmt(type), "type");

            if (giving)
                paymentInfo.PreferredGivingType = type;
            else
                paymentInfo.PreferredPaymentType = type;
            db.SubmitChanges();
        }

        private int CreateCreditCardVault(Person person, string cardNumber, string expiration)
        {
            var createCreditCardVaultRequest = new CreateCreditCardVaultRequest(
                _userName,
                _password,
                new CreditCard
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    CardNumber = cardNumber,
                    Expiration = expiration,
                    BillingAddress = new BillingAddress
                    {
                        Address1 = person.PrimaryAddress,
                        City = person.PrimaryCity,
                        State = person.PrimaryState,
                        Zip = person.PrimaryZip,
                        Email = person.EmailAddress,
                        Phone = person.HomePhone ?? person.CellPhone
                    }
                });

            var response = createCreditCardVaultRequest.Execute();
            if (response.ResponseStatus != ResponseStatus.Approved)
                throw new Exception(
                    "TransNational failed to create the credit card for people id: {0}".Fmt(person.PeopleId));

            return response.VaultId.ToInt();
        }

        private void UpdateCreditCardVault(int vaultId, Person person, string cardNumber, string expiration)
        {
            var updateCreditCardVaultRequest = new UpdateCreditCardVaultRequest(
                _userName,
                _password,
                vaultId.ToString(CultureInfo.InvariantCulture),
                new CreditCard
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    CardNumber = cardNumber,
                    Expiration = expiration,
                    BillingAddress = new BillingAddress
                    {
                        Address1 = person.PrimaryAddress,
                        City = person.PrimaryCity,
                        State = person.PrimaryState,
                        Zip = person.PrimaryZip,
                        Email = person.EmailAddress,
                        Phone = person.HomePhone ?? person.CellPhone
                    }
                });

            var response = updateCreditCardVaultRequest.Execute();
            if (response.ResponseStatus != ResponseStatus.Approved)
                throw new Exception(
                    "TransNational failed to update the credit card for people id: {0}".Fmt(person.PeopleId));
        }

        private void UpdateCreditCardVault(int vaultId, Person person, string expiration)
        {
            var updateCreditCardVaultRequest = new UpdateCreditCardVaultRequest(
                _userName, 
                _password,
                vaultId.ToString(CultureInfo.InvariantCulture), expiration);

            var response = updateCreditCardVaultRequest.Execute();
            if (response.ResponseStatus != ResponseStatus.Approved)
                throw new Exception(
                    "TransNational failed to update the credit card expiration date for people id: {0}".Fmt(
                        person.PeopleId));
        }

        private int CreateAchVault(Person person, string accountNumber, string routingNumber)
        {
            var createAchVaultRequest = new CreateAchVaultRequest(
                _userName,
                _password,
                new Ach
                {
                    NameOnAccount = person.Name,
                    AccountNumber = accountNumber,
                    RoutingNumber = routingNumber,
                    BillingAddress = new BillingAddress
                    {
                        Address1 = person.PrimaryAddress,
                        City = person.PrimaryCity,
                        State = person.PrimaryState,
                        Zip = person.PrimaryZip,
                        Email = person.EmailAddress,
                        Phone = person.HomePhone ?? person.CellPhone
                    }
                });

            var response = createAchVaultRequest.Execute();
            if (response.ResponseStatus != ResponseStatus.Approved)
                throw new Exception(
                    "TransNational failed to create the ach account for people id: {0}".Fmt(person.PeopleId));

            return response.VaultId.ToInt();
        }

        private void UpdateAchVault(int vaultId, Person person)
        {
            var updateAchVaultRequest = new UpdateAchVaultRequest(
                _userName,
                _password,
                vaultId.ToString(CultureInfo.InvariantCulture),
                person.Name,
                new BillingAddress
                {
                    Address1 = person.PrimaryAddress,
                    City = person.PrimaryCity,
                    State = person.PrimaryState,
                    Zip = person.PrimaryZip,
                    Email = person.EmailAddress,
                    Phone = person.HomePhone ?? person.CellPhone
                });

            var response = updateAchVaultRequest.Execute();
            if (response.ResponseStatus != ResponseStatus.Approved)
                throw new Exception(
                    "TransNational failed to update the ach account for people id: {0}".Fmt(person.PeopleId));
        }

        private void UpdateAchVault(int vaultId, Person person, string accountNumber, string routingNumber)
        {
            var updateAchVaultRequest = new UpdateAchVaultRequest(
                _userName,
                _password,
                vaultId.ToString(CultureInfo.InvariantCulture),
                new Ach
                {
                    NameOnAccount = person.Name,
                    AccountNumber = accountNumber,
                    RoutingNumber = routingNumber,
                    BillingAddress = new BillingAddress
                    {
                        Address1 = person.PrimaryAddress,
                        City = person.PrimaryCity,
                        State = person.PrimaryState,
                        Zip = person.PrimaryZip,
                        Email = person.EmailAddress,
                        Phone = person.HomePhone ?? person.CellPhone
                    }
                });

            var response = updateAchVaultRequest.Execute();
            if (response.ResponseStatus != ResponseStatus.Approved)
                throw new Exception(
                    "TransNational failed to update the ach account for people id: {0}".Fmt(person.PeopleId));
        }

        public void RemoveFromVault(int peopleId)
        {
            var person = db.LoadPersonById(peopleId);
            var paymentInfo = person.PaymentInfo();
            if (paymentInfo == null)
                return;

            if (paymentInfo.TbnCardVaultId.HasValue)
                DeleteVault(paymentInfo.TbnCardVaultId.GetValueOrDefault(), person);

            if (paymentInfo.TbnBankVaultId.HasValue)
                DeleteVault(paymentInfo.TbnBankVaultId.GetValueOrDefault(), person);

            // clear out local record and save changes.
            paymentInfo.TbnCardVaultId = null;
            paymentInfo.TbnBankVaultId = null;
            paymentInfo.MaskedCard = null;
            paymentInfo.MaskedAccount = null;
            paymentInfo.Ccv = null;
            db.SubmitChanges();
        }

        private void DeleteVault(int vaultId, Person person)
        {
            var deleteVaultRequest = new DeleteVaultRequest(
                _userName, 
                _password,
                vaultId.ToString(CultureInfo.InvariantCulture));

            var response = deleteVaultRequest.Execute();
            if (response.ResponseStatus != ResponseStatus.Approved)
                throw new Exception("TransNational failed to delete the vault for people id: {0}".Fmt(person.PeopleId));
        }

        public TransactionResponse VoidCreditCardTransaction(string reference)
        {
            return Void(reference);
        }

        public TransactionResponse VoidCheckTransaction(string reference)
        {
            return Void(reference);
        }

        private TransactionResponse Void(string reference)
        {
            var voidRequest = new VoidRequest(_userName, _password, reference);
            var response = voidRequest.Execute();

            return new TransactionResponse
            {
                Approved = response.ResponseStatus == ResponseStatus.Approved,
                AuthCode = response.AuthCode,
                Message = response.ResponseText,
                TransactionId = response.TransactionId
            };
        }

        public TransactionResponse RefundCreditCard(string reference, Decimal amt, string lastDigits = "")
        {
            return Refund(reference, amt);
        }

        public TransactionResponse RefundCheck(string reference, Decimal amt, string lastDigits = "")
        {
            return Refund(reference, amt);
        }

        private TransactionResponse Refund(string reference, Decimal amount)
        {
            var refundRequest = new RefundRequest(_userName, _password, reference, amount);
            var response = refundRequest.Execute();

            return new TransactionResponse
            {
                Approved = response.ResponseStatus == ResponseStatus.Approved,
                AuthCode = response.AuthCode,
                Message = response.ResponseText,
                TransactionId = response.TransactionId
            };
        }

        public TransactionResponse PayWithCreditCard(int peopleId, decimal amt, string cardnumber, string expires,
            string description, int tranid, string cardcode, string email, string first, string last, string addr,
            string city, string state, string zip, string phone)
        {
            var creditCardSaleRequest = new CreditCardSaleRequest(
                _userName,
                _password,
                new CreditCard
                {
                    FirstName = first,
                    LastName = last,
                    CardNumber = cardnumber,
                    Expiration = expires,
                    CardCode = cardcode,
                    BillingAddress = new BillingAddress
                    {
                        Address1 = addr,
                        City = city,
                        State = state,
                        Zip = zip,
                        Email = email,
                        Phone = phone
                    }
                },
                amt,
                tranid.ToString(CultureInfo.InvariantCulture),
                description,
                peopleId.ToString(CultureInfo.InvariantCulture));

            var response = creditCardSaleRequest.Execute();

            return new TransactionResponse
            {
                Approved = response.ResponseStatus == ResponseStatus.Approved,
                AuthCode = response.AuthCode,
                Message = response.ResponseText,
                TransactionId = response.TransactionId
            };
        }

        public TransactionResponse PayWithCheck(int peopleId, decimal amt, string routing, string acct,
            string description, int tranid, string email, string first, string middle, string last, string suffix,
            string addr, string city, string state, string zip, string phone)
        {
            var achSaleRequest = new AchSaleRequest(
                _userName,
                _password,
                new Ach
                {
                    NameOnAccount = string.Format("{0} {1}", first, last),
                    AccountNumber = acct,
                    RoutingNumber = routing,
                    BillingAddress = new BillingAddress
                    {
                        Address1 = addr,
                        City = city,
                        State = state,
                        Zip = zip,
                        Email = email,
                        Phone = phone
                    }
                },
                amt,
                tranid.ToString(CultureInfo.InvariantCulture),
                description,
                peopleId.ToString(CultureInfo.InvariantCulture));

            var response = achSaleRequest.Execute();

            return new TransactionResponse
            {
                Approved = response.ResponseStatus == ResponseStatus.Approved,
                AuthCode = response.AuthCode,
                Message = response.ResponseText,
                TransactionId = response.TransactionId
            };
        }

        public TransactionResponse PayWithVault(int peopleId, decimal amt, string description, int tranid, string type)
        {
            var person = db.LoadPersonById(peopleId);
            var paymentInfo = person.PaymentInfo();
            if (paymentInfo == null)
                return new TransactionResponse
                {
                    Approved = false,
                    Message = "missing payment info",
                };

            if (type == PaymentType.CreditCard) // credit card
                return ChargeCreditCardVault(paymentInfo.TbnCardVaultId.GetValueOrDefault(), peopleId, amt, tranid,
                    description);
            else // bank account
                return ChargeAchVault(paymentInfo.TbnBankVaultId.GetValueOrDefault(), peopleId, amt, tranid, description);

        }

        private TransactionResponse ChargeCreditCardVault(int vaultId, int peopleId, decimal amount, int tranid,
            string description)
        {
            var creditCardVaultSaleRequest = new CreditCardVaultSaleRequest(
                _userName,
                _password,
                vaultId.ToString(CultureInfo.InvariantCulture),
                amount,
                tranid.ToString(CultureInfo.InvariantCulture),
                description,
                peopleId.ToString(CultureInfo.InvariantCulture));

            var response = creditCardVaultSaleRequest.Execute();

            return new TransactionResponse
            {
                Approved = response.ResponseStatus == ResponseStatus.Approved,
                AuthCode = response.AuthCode,
                Message = response.ResponseText,
                TransactionId = response.TransactionId
            };
        }

        private TransactionResponse ChargeAchVault(int vaultId, int peopleId, decimal amount, int tranid,
            string description)
        {
            var achVaultSaleRequest = new AchVaultSaleRequest(
                _userName,
                _password,
                vaultId.ToString(CultureInfo.InvariantCulture),
                amount,
                tranid.ToString(CultureInfo.InvariantCulture),
                description,
                peopleId.ToString(CultureInfo.InvariantCulture));

            var response = achVaultSaleRequest.Execute();

            return new TransactionResponse
            {
                Approved = response.ResponseStatus == ResponseStatus.Approved,
                AuthCode = response.AuthCode,
                Message = response.ResponseText,
                TransactionId = response.TransactionId
            };
        }

        public BatchResponse GetBatchDetails(DateTime start, DateTime end)
        {
            var batchTransactions = new List<BatchTransaction>();

            // settled sale, capture, credit & refund transactions.
            var queryRequest = new QueryRequest(
                _userName,
                _password,
                start,
                end,
                new List<TransNational.Query.Condition> {TransNational.Query.Condition.Complete},
                new List<ActionType> {ActionType.Settle, ActionType.Sale, ActionType.Capture, ActionType.Credit, ActionType.Refund}); 

            var response = queryRequest.Execute();

            BuildBatchTransactionsList(response.Transactions, ActionType.Sale, batchTransactions);
            BuildBatchTransactionsList(response.Transactions, ActionType.Capture, batchTransactions);
            BuildBatchTransactionsList(response.Transactions, ActionType.Credit, batchTransactions);
            BuildBatchTransactionsList(response.Transactions, ActionType.Refund, batchTransactions);

            return new BatchResponse(batchTransactions);
        }

        private void BuildBatchTransactionsList(IEnumerable<TransNational.Query.Transaction> transactions, ActionType originalActionType, List<BatchTransaction> batchTransactions)
        {
            var transactionList = transactions.Where(t => t.Actions.Any(a => a.ActionType == originalActionType));

            foreach (var transaction in transactionList)
            {
                var originalAction = transaction.Actions.FirstOrDefault(a => a.ActionType == originalActionType);
                var settleAction = transaction.Actions.FirstOrDefault(a => a.ActionType == ActionType.Settle);

                // need to make sure that both the settle action and the original action (sale, capture, credit or refund) are present before proceeding.
                if (originalAction != null && settleAction != null)
                {
                    // prevent adding the same batch transaction more than once.
                    if (batchTransactions.All(b => b.TransactionId != transaction.OrderId.ToInt()))
                    {
                        batchTransactions.Add(new BatchTransaction
                        {
                            TransactionId = transaction.OrderId.ToInt(),
                            Reference = transaction.TransactionId,
                            BatchReference = settleAction.BatchId,
                            TransactionType = GetTransactionType(originalActionType),
                            BatchType = GetBatchType(transaction.TransactionType),
                            Name = transaction.Name,
                            Amount = settleAction.Amount,
                            Approved = originalAction.Success,
                            Message = originalAction.ResponseText,
                            TransactionDate = originalAction.Date,
                            SettledDate = settleAction.Date,
                            LastDigits = transaction.LastDigits
                        });
                    }
                }
            }
            
        }

        private TransactionType GetTransactionType(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.Sale:
                case ActionType.Capture:
                    return TransactionType.Charge;
                case ActionType.Credit:
                    return TransactionType.Credit;
                case ActionType.Refund:
                    return TransactionType.Refund;
                default:
                    return TransactionType.Unknown;
            }
        }

        /// <summary>
        /// TransNational calls their payment method type transaction type
        /// so that's what we use to figure out the batch type.
        /// </summary>
        /// <param name="transactionType"></param>
        /// <returns></returns>
        private BatchType GetBatchType(TransNational.Query.TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransNational.Query.TransactionType.CreditCard:
                    return BatchType.CreditCard;
                case TransNational.Query.TransactionType.Ach:
                    return BatchType.Ach;
                default:
                    return BatchType.Unknown;
            }
        }

        public DataSet VirtualCheckRejects(DateTime startdt, DateTime enddt)
        {
            //var queryRequest = new QueryRequest(userName,
            //                                    password,
            //                                    startdt,
            //                                    enddt,
            //                                    new List<Internal.Query.Condition> { Internal.Query.Condition.Failed },
            //                                    new List<TransactionType> { TransactionType.Ach },
            //                                    new List<ActionType> { ActionType.CheckReturn, ActionType.CheckLateReturn });

            //var response = queryRequest.Execute();

            return null;
        }

        public DataSet VirtualCheckRejects(DateTime rejectdate)
        {
            return null;
        }

        public bool CanVoidRefund
        {
            get { return true; }
        }

        public bool CanGetSettlementDates
        {
            get { return true; }
        }

        public bool CanGetBounces
        {
            get { return false; }
        }
    }
}
