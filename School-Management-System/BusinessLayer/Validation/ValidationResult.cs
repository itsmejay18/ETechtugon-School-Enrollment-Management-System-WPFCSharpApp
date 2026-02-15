using System;
using System.Collections.Generic;

namespace School_Management_System.BusinessLayer.Validation
{
    public sealed class ValidationResult
    {
        private readonly List<string> _errors = new List<string>();

        public bool IsValid
        {
            get { return _errors.Count == 0; }
        }

        public IReadOnlyList<string> Errors
        {
            get { return _errors; }
        }

        public void Add(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                _errors.Add(error.Trim());
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _errors);
        }
    }
}

