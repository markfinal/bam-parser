using Bam.Core;
namespace bison
{
    [Bam.Core.SettingsExtensions(typeof(DefaultSettings.DefaultSettingsExtensions))]
    interface IBisonSettings :
        Bam.Core.ISettingsBase
    {
    }
}
