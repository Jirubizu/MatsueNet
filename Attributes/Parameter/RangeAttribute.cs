using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace MatsueNet.Attributes.Parameter
{
    public class RangeAttribute : ParameterPreconditionAttribute
    {
        private readonly int _min;
        private readonly int _max;

        public RangeAttribute(int min, int max)
        {
            this._min = min;
            this._max = max;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            if (value is int i && i >= _min && i <= _max)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            return Task.FromResult(PreconditionResult.FromError($"Parameter `{parameter.Name}` is out of range."));
        }
    }
}