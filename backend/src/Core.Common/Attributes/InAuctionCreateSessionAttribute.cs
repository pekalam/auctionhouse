using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Users;
using Core.Common.Exceptions;
using Core.Common.Query;


namespace Core.Common.Attributes
{
    public class InAuctionCreateSessionRemoveAttribute : InAuctionCreateSessionAttribute
    {
        public new Action<IImplProvider, ICommand> PostHandleAttributeStrategy =>
            new Action<IImplProvider, ICommand>(RemoveAuctionCreateSession);

        private static void RemoveAuctionCreateSession(IImplProvider implProvider, ICommand command)
        {
            if (_auctionCreateSessionCommandProperties.ContainsKey(command.GetType()))
            {
                var auctionCreateSessionService = implProvider.Get<IAuctionCreateSessionService>();
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
                var interfaces = member.CmdType.GetInterfaces();
                _auctionCreateSessionCommandProperties[member.CmdType] = member.PropertyInfo;
            }
        }

        private static void AddAuctionCreateSessionToCommand(IImplProvider implProvider, ICommand command)
        {
            if (_auctionCreateSessionCommandProperties.ContainsKey(command.GetType()))
            {
                var auctionCreateSessionService = implProvider.Get<IAuctionCreateSessionService>();
                var session = auctionCreateSessionService.GetExistingSession();
                _auctionCreateSessionCommandProperties[command.GetType()].SetValue(command, session);
            }
        }

        private static void SaveAuctionCreateSession(IImplProvider implProvider, ICommand command)
        {
            if (_auctionCreateSessionCommandProperties.ContainsKey(command.GetType()))
            {
                var auctionCreateSessionService = implProvider.Get<IAuctionCreateSessionService>();
                var session = _auctionCreateSessionCommandProperties[command.GetType()].GetValue(command) as AuctionCreateSession; 
                auctionCreateSessionService.SaveSession(session);
            }
        }
    }
}