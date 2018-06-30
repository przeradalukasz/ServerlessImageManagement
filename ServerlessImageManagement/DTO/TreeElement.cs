using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessImageManagement.DTO
{
    public class TreeElement
    {
        public string Name { get; set; }
        public string AbsolutePath { get; set; }
        public string RelativePath { get; set; }
        public ICollection<TreeElement> Children { get; set; }
    }
}
