using System.Data;
using Dapper;

namespace ContactManagerApplication.Mappings
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }

        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                string dateString when DateOnly.TryParse(dateString, out var dateOnly) => dateOnly,
                _ => throw new ArgumentException($"Cannot convert {value?.GetType().Name ?? "null"} to DateOnly", nameof(value))
            };
        }
    }
}
