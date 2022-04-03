using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBlobStorageFileAccessor
{
    public class Result<T>
    {
        public Result(T? value, List<string> errors)
        {
            this.Value = value;
            this.Errors = errors;
        }

        public Result(T? value, string error)
        {
            this.Value = value;
            this.Errors.Add(error);
        }

        public Result(T? value)
        {
            this.Value = value;
        }

        public T? Value { get; set; }

        public List<string> Errors { get; set; } = new List<string>();

        public string? Error => Errors.Any() ? Errors.First() : null;

        public bool HasValue => (Value != null);
        public bool HasErrors => Errors.Any();
    }
}
