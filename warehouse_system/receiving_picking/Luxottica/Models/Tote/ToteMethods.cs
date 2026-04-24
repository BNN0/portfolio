using Luxottica.Models.NewTote;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Luxottica.Models.Tote
{
    public class ToteMethods
    {
        public static string RemoveNonNumericChars(string input)
        {
            // Reemplaza todos los caracteres que no son dígitos con una cadena vacía.
            string numericOnly = new string(Array.FindAll(input.ToCharArray(), char.IsDigit));
            return numericOnly;
        }

        public static bool ValidRequest(NewToteModel request)
        {
            if (request == null)
            {
                return false;
            }
            if (!ValidateCamFormat(request.camId))
            {
                return false;
            }

            return true;
        }

        public static bool ValidateCamFormat(string input)
        {
            // Patrón regex para validar el formato "Cam" seguido de dos dígitos
            string pattern = @"^Cam\d{2}$";

            // Verificar si el input coincide con el patrón
            return Regex.IsMatch(input, pattern);
        }

        private static bool ValidateTimestampFormat(string timestamp)
        {
            string format = "yyyyMMddHHmmssfff";

            if (DateTime.TryParseExact(timestamp, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
