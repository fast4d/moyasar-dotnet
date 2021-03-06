using System;
using System.Linq;
using Moyasar.Abstraction;
using Moyasar.Exceptions;
using Moyasar.Models;
using Moyasar.Services;

namespace MoyasarEasyCli
{
    public static class PaymentProcessor
    {
        public static void InitiatePaymentProcessor()
        {
            while (!PrintOutPaymentMenu()) { }
        }
        
        public static bool PrintOutPaymentMenu()
        {
            Program.ClearPrintOutWelcomeDetails();
            Console.WriteLine();
            Console.WriteLine("[1] Create Payment [Not Available]");
            Console.WriteLine("[2] Fetch Payment");
            Console.WriteLine("[3] List Payments");
            Console.WriteLine("[0] Go Back");
            Console.WriteLine();
            Console.Write("Please choose an option: ");

            var option = Console.ReadLine();
            try
            {
                if (option != null) return ProcessPaymentMenuOption(option);
                throw new Exception();
            }
            catch
            {
                Console.WriteLine();
                Console.WriteLine("Error: Invalid option");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }

            return false;
        }

        private static bool ProcessPaymentMenuOption(string option)
        {
            switch (option)
            {
                case "0":
                    return true;
                case "1":
//                    CreatePayment();
                    break;
                case "2":
                    FetchPayment();
                    break;
                case "3":
                    ListPayments();
                    break;
                default:
                    throw new Exception();
            }
            
            return false;
        }

        private static void FetchPayment()
        {
            Program.ClearPrintOutWelcomeDetails();
            Console.WriteLine();

            var id = "";
            while (string.IsNullOrEmpty(id))
            {
                Console.Write("Payment Id: ");
                id = Console.ReadLine();
            }

            Payment payment;
            try
            {
                payment = Payment.Fetch(id);
            }
            catch (NetworkException)
            {
                Console.WriteLine("Could not connect to Internet");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            catch (ApiException e)
            {
                Console.WriteLine($"Api Exception:\n{e}");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            PrintOutPayment(payment);
        }
        
        private static void ListPayments()
        {
            Program.ClearPrintOutWelcomeDetails();
            Console.WriteLine();
            Console.WriteLine("Retrieving payments");
            
            var payments = Payment.List();

            if (!payments.Items.Any())
            {
                Console.WriteLine("No payments found");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                while (true)
                {
                    Program.ClearPrintOutWelcomeDetails();
                    Console.WriteLine();

                    Console.WriteLine($"Page {payments.CurrentPage.ToString()} of {payments.TotalPages.ToString()}");
                    
                    for (int i = 0; i < payments.Items.Count; ++i)
                    {
                        Console.WriteLine($"[{i + 1}] {payments.Items[i].Id}");
                    }
                    
                    Console.WriteLine("[0] Go Back");
                    
                    if (payments.NextPage.HasValue)
                    {
                        Console.WriteLine("[>] Next Page");
                    }
                    
                    if (payments.PreviousPage.HasValue)
                    {
                        Console.WriteLine("[<] Previous Page");
                    }

                    var choice = "0";
                    while (true)
                    {
                        Console.WriteLine();
                        Console.Write("Please choose an option: ");
                        try
                        {
                            choice = Console.ReadLine();
                            break;
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    if (choice == "0") break;
                    if (choice == ">" && payments.NextPage.HasValue)
                    {
                        payments = payments.GetNextPage();
                    }
                    else if(choice == "<" && payments.PreviousPage.HasValue)
                    {
                        payments = payments.GetPreviousPage();
                    }
                    else if (int.Parse(choice) < 0 || int.Parse(choice) > payments.Items.Count)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Error: Invalid option");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    else
                    {
                        PrintOutPayment(payments.Items[int.Parse(choice) - 1]);
                    }
                }
            }
        }
        
        public static void PrintOutPayment(Payment payment)
        {
            Program.ClearPrintOutWelcomeDetails();
            Console.WriteLine();
            
            Console.WriteLine($"Id: {payment.Id}");
            Console.WriteLine($"Status: {payment.Status}");
            Console.WriteLine($"Amount: {payment.Amount.ToString()}");
            Console.WriteLine($"Formatted Amount: {payment.FormattedAmount}");
            Console.WriteLine($"Fee: {payment.Fee.ToString()}");
            Console.WriteLine($"Formatted Fee: {payment.FormattedFee}");
            
            if (payment.RefundedAt.HasValue)
            {
                Console.WriteLine($"Refunded Amount: {payment.RefundedAmount.ToString()}");
                Console.WriteLine($"Formatted Refunded Amount: {payment.FormattedRefundedAmount}");
                Console.WriteLine($"Refunded At: {payment.RefundedAt?.ToString("O")}");
            }
            
            Console.WriteLine($"Currency: {payment.Currency}");
            Console.WriteLine($"Description: {payment.Description}");

            if (payment.InvoiceId != null)
            {
                Console.WriteLine($"Invoice Id: {payment.InvoiceId}");
            }
            
            if (payment.Ip != null)
            {
                Console.WriteLine($"IP Address: {payment.Ip}");
            }
            
            if (payment.CallbackUrl != null)
            {
                Console.WriteLine($"Callback URL: {payment.CallbackUrl}");
            }
            
            if (payment.CreatedAt.HasValue)
            {
                Console.WriteLine($"Refunded At: {payment.CreatedAt?.ToString("O")}");
            }
            
            if (payment.UpdatedAt.HasValue)
            {
                Console.WriteLine($"Refunded At: {payment.UpdatedAt?.ToString("O")}");
            }

            Console.WriteLine("Payment Method:");
            ShowPaymentMethod(payment.Source);
            
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void ShowPaymentMethod(IPaymentMethod paymentMethod)
        {
            switch (paymentMethod)
            {
                case CreditCard cc:
                    Console.WriteLine($"\t- Name: {cc.Name}");
                    Console.WriteLine($"\t- Number: {cc.Number}");
                    Console.WriteLine($"\t- Company: {cc.Company}");
                    Console.WriteLine($"\t- Message: {cc.Message}");
                    Console.WriteLine($"\t- Transaction URL: {cc.TransactionUrl}");
                    break;
                case Sadad sadad:
                    Console.WriteLine($"\t- UserName: {sadad.UserName}");
                    Console.WriteLine($"\t- Error Code: {sadad.ErrorCode ?? "None"}");
                    Console.WriteLine($"\t- Message: {sadad.Message ?? "None"}");
                    Console.WriteLine($"\t- Transaction Id: {sadad.TransactionId ?? "None"}");
                    Console.WriteLine($"\t- Transaction Url: {sadad.TransactionUrl ?? "None"}");
                    break;
            }
        }
    }
}