using System.Collections;
using System.Collections.Generic;

namespace RecognitionOrderValidator
{
    public class RecognitionOrder
    {
        public string PhotosSource { get; set; }
        public IEnumerable<string> PatternFaces { get; set; }
        public string DestinationFolder { get; set; }
        public string RecognitionName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
    }
}
