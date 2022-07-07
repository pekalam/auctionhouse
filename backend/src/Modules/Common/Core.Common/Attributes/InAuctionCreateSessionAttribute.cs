using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Core.Common.Exceptions;
using Core.Common.Query;
using Microsoft.Extensions.Logging;


namespace Core.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SaveTempPathAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AuctionImageAttribute : Attribute
    {
    }

    public class SaveTempAuctionImageAttribute : Attribute, ICommandAttribute
    {
        internal static Dictionary<Type, PropertyInfo> _auctionImagePathCommandProperties;
        internal static Dictionary<Type, PropertyInfo> _auctionImageAccessorCommandProperties;

        public Action<IImplProvider, ICommand> PreHandleAttributeStrategy => SaveImage;
        public Action<IImplProvider, ICommand> PostHandleAttributeStrategy => null;
        public int Order => 1;

        static SaveTempAuctionImageAttribute()
        {
            LoadImagePathCommandMembers("Core.Command");
        }

        internal static void LoadImagePathCommandMembers(string cmdAssembly)
        {
            var imgPathMembers = Assembly.Load(cmdAssembly)
                .GetTypes()
                .Where(type => type.BaseType == typeof(ICommand))
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

            var imgAccessorMembers = Assembly.Load(cmdAssembly)
                .GetTypes()
                .Where(type => type.BaseType == typeof(ICommand))
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

        internal static void SaveImage(IImplProvider implProvider, ICommand commandBase)
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


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InAuctionCreateSessionRemoveAttribute : InAuctionCreateSessionAttribute
    {
        public new Action<IImplProvider, ICommand> PostHandleAttributeStrategy =>
            new Action<IImplProvider, ICommand>(RemoveAuctionCreateSession);

        private static void RemoveAuctionCreateSession(IImplProvider implProvider, ICommand commandBase)
        {
            if (_auctionCreateSessionCommandProperties.ContainsKey(commandBase.GetType()))
            {
                var auctionCreateSessionService = implProvider.Get<IAuctionCreateSessionStore>();
                auctionCreateSessionService.RemoveSession();
            }
        }
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InAuctionCreateSessionAttribute : Attribute, ICommandAttribute
    {
        internal static Dictionary<Type, PropertyInfo> _auctionCreateSessionCommandProperties;

        public Action<IImplProvider, ICommand> PreHandleAttributeStrategy =>
            new Action<IImplProvider, ICommand>(AddAuctionCreateSessionToCommand);

        public Action<IImplProvider, ICommand> PostHandleAttributeStrategy =>
            new Action<IImplProvider, ICommand>(SaveAuctionCreateSession);

        public int Order => 1;

        static InAuctionCreateSessionAttribute()
        {
            LoadAuctionCreateSessionCommandMembers("Core.Command");
        }

        internal static void LoadAuctionCreateSessionCommandMembers(params string[] assemblyNames)
        {
            var commandMembers = assemblyNames.Select(s => Assembly.Load(s))
                .Select(assembly =>
                    assembly.GetTypes()
                        .Where(type => type.BaseType == typeof(ICommand))
                        .Select(type => type.GetProperties())
                        .SelectMany(infos => infos)
                        .Where(info =>
                            info.CanWrite &&
                            info.PropertyType.Equals(typeof(AuctionCreateSession)))
                        .Select(info => new
                        {
                            CmdType = info.DeclaringType,
                            PropertyInfo = info
                        })
                )
                .SelectMany(enumerable => enumerable);

            _auctionCreateSessionCommandProperties = new Dictionary<Type, PropertyInfo>();

            foreach (var member in commandMembers)
            {
                _auctionCreateSessionCommandProperties[member.CmdType] = member.PropertyInfo;
            }
        }

        private static void AddAuctionCreateSessionToCommand(IImplProvider implProvider, ICommand commandBase)
        {
            if (_auctionCreateSessionCommandProperties.ContainsKey(commandBase.GetType()))
            {
                var auctionCreateSessionService = implProvider.Get<IAuctionCreateSessionStore>();
                var session = auctionCreateSessionService.GetExistingSession();
                _auctionCreateSessionCommandProperties[commandBase.GetType()].SetValue(commandBase, session);
            }
        }

        private static void SaveAuctionCreateSession(IImplProvider implProvider, ICommand commandBase)
        {
            if (_auctionCreateSessionCommandProperties.ContainsKey(commandBase.GetType()))
            {
                var auctionCreateSessionService = implProvider.Get<IAuctionCreateSessionStore>();
                var session =
                    _auctionCreateSessionCommandProperties[commandBase.GetType()].GetValue(commandBase) as AuctionCreateSession;
                auctionCreateSessionService.SaveSession(session);
            }
        }
    }
}