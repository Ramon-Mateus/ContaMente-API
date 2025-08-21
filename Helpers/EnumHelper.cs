using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class EnumHelper
{
    public static string GetDisplayName(this Enum value)
    {
        var memberInfo = value.GetType().GetMember(value.ToString());

        if (memberInfo.Length > 0)
        {
            var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>(false);

            if (displayAttribute != null)
            {
                return displayAttribute.Name!;
            }
        }

        return value.ToString();
    }
}