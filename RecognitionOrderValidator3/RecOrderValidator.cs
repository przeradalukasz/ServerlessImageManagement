using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;


namespace RecognitionOrderValidator
{
    public class RecOrderValidator : IRecOrderValidator
    {
        private readonly Regex Regex = new Regex(@"^(\+[0-9]{9})$", RegexOptions.Compiled);
        private readonly EmailAddressAttribute EmailAddressAttribute = new EmailAddressAttribute();

        public bool IsValid(RecognitionOrder recognitionOrder)
        {
            if (string.IsNullOrWhiteSpace(recognitionOrder.DestinationFolder))
                return false;
            if (string.IsNullOrWhiteSpace(recognitionOrder.PhotosSource))
                return false;
            if (!recognitionOrder.PatternFaces.Any())
                return false;
            if (string.IsNullOrWhiteSpace(recognitionOrder.EmailAddress) || !EmailAddressAttribute.IsValid(recognitionOrder.EmailAddress))
                return false;
            if (!Regex.Match(recognitionOrder.PhoneNumber).Success)
                return false;
            return true;
        }
    }
}
