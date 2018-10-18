using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Service.PayInvoicePortal.Constants
{
    /// <summary>
    /// Class for storing all possible error codes that may happen in Api.
    /// </summary>
    public static class PayInvoicePortalApiErrorCodes
    {
        public static class Signup
        {
            /// <summary>
            /// Merchant with such id exist
            /// </summary>
            public const string MerchantExist = nameof(MerchantExist);

            /// <summary>
            /// Merchant with such email exist
            /// </summary>
            public const string MerchantEmailExist = nameof(MerchantEmailExist);

            /// <summary>
            /// Employee with such email exist
            /// </summary>
            public const string EmployeeEmailExist = nameof(EmployeeEmailExist);

            /// <summary>
            /// Email sending problem occured
            /// </summary>
            public const string EmailNotSent = nameof(EmailNotSent);

            public static class EmailConfirmation
            {
                public const string InvalidToken = nameof(InvalidToken);
            }
        }

        public static class ChangePassword
        {
            /// <summary>
            /// Current password is invalid
            /// </summary>
            public const string InvalidCurrentPassword = nameof(InvalidCurrentPassword);
        }

        /// <summary>
        /// Unexpected error - contacting support and investigation required
        /// </summary>
        public const string UnexpectedError = nameof(UnexpectedError);
    }
}
