using Common.Application;
using Common.Application.Commands;
using Core.Common;
using System.Reflection;


namespace Auctions.Application.CommandAttributes
{
    public class SaveTempAuctionImageAttribute : Attribute, ICommandAttribute
    {
        internal static Dictionary<Type, PropertyInfo> _auctionImagePathCommandProperties;
        internal static Dictionary<Type, PropertyInfo> _auctionImageAccessorCommandProperties;

        public Action<IImplProvider, CommandContext, ICommand> PreHandleAttributeStrategy => SaveImage;
        public Action<IImplProvider, CommandContext, ICommand> PostHandleAttributeStrategy => null;
        public int Order => 1;

        public static void LoadImagePathCommandMembers(params string[] cmdAssemblies)
        {
            var imgPathMembers = cmdAssemblies.SelectMany(n => Assembly.Load(n)
                .GetTypes())
                .Where(type => type.GetInterfaces().Contains(typeof(ICommand)))
                .Where(type => type.GetCustomAttributes()
                                   .Count(attribute => attribute.GetType() == typeof(SaveTempAuctionImageAttribute)) >
                               0)
                .Select(type => type.GetProperties())
                .SelectMany(infos => infos)
                .Where(info => info.CanWrite && info.GetCustomAttribute<SaveTempPathAttribute>() != null &&
                               info.PropertyType == typeof(string))
                .Select(info => new
                {
                    CmdType = info.DeclaringType,
                    PropertyInfo = info
                });

            var imgAccessorMembers = cmdAssemblies.SelectMany(n => Assembly.Load(n)
                .GetTypes())
                .Where(type => type.GetInterfaces().Contains(typeof(ICommand)))
                .Where(type => type.GetCustomAttributes()
                                   .Count(attribute => attribute.GetType() == typeof(SaveTempAuctionImageAttribute)) >
                               0)
                .Select(type => type.GetProperties())
                .SelectMany(infos => infos)
                .Where(info => info.CanWrite && info.GetCustomAttribute<AuctionImageAttribute>() != null &&
                               info.PropertyType == typeof(IFileStreamAccessor))
                .Select(info => new
                {
                    CmdType = info.DeclaringType,
                    PropertyInfo = info
                });

            _auctionImagePathCommandProperties = new Dictionary<Type, PropertyInfo>();
            _auctionImageAccessorCommandProperties = new Dictionary<Type, PropertyInfo>();

            foreach (var member in imgPathMembers)
            {
                _auctionImagePathCommandProperties[member.CmdType] = member.PropertyInfo;
                if (imgAccessorMembers.Count(arg => arg.CmdType == member.CmdType) == 0)
                {
                    throw new Exception(
                        $"Command {member.CmdType} decorated with {nameof(SaveTempAuctionImageAttribute)} attribute doesn't contain property decorated with{nameof(AuctionImageAttribute)} which should correspond to {member.PropertyInfo.Name} property");
                }
            }

            foreach (var member in imgAccessorMembers)
            {
                _auctionImageAccessorCommandProperties[member.CmdType] = member.PropertyInfo;
                if (imgPathMembers.Count(arg => arg.CmdType == member.CmdType) == 0)
                {
                    throw new Exception(
                        $"Command {member.CmdType} decorated with {nameof(SaveTempAuctionImageAttribute)} attribute doesn't contain property decorated with{nameof(SaveTempPathAttribute)} which should correspond to {member.PropertyInfo.Name} property");
                }
            }
        }

        internal static void SaveImage(IImplProvider implProvider, CommandContext ctx, ICommand commandBase)
        {
            if (_auctionImagePathCommandProperties.ContainsKey(commandBase.GetType()))
            {
                var tempFileService = implProvider.Get<ITempFileService>();
                var streamAccessor = (IFileStreamAccessor)_auctionImageAccessorCommandProperties[commandBase.GetType()].GetValue(commandBase);
                var stream = streamAccessor.GetStream();
                var tempFile = tempFileService.SaveAsTempFile(stream);
                stream.Close();
                _auctionImageAccessorCommandProperties[commandBase.GetType()].SetValue(commandBase, null);
                _auctionImagePathCommandProperties[commandBase.GetType()].SetValue(commandBase, tempFile);
            }
        }
    }
}