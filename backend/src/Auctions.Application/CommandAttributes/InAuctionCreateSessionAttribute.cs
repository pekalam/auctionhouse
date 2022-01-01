﻿using Auctions.Domain;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using System.Reflection;


namespace Auctions.Application.CommandAttributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InAuctionCreateSessionAttribute : Attribute, ICommandAttribute
    {
        internal static Dictionary<Type, PropertyInfo> _auctionCreateSessionCommandProperties;

        public Action<IImplProvider, ICommand> PreHandleAttributeStrategy =>
            new Action<IImplProvider, ICommand>(AddAuctionCreateSessionToCommand);

        public Action<IImplProvider, ICommand> PostHandleAttributeStrategy =>
            new Action<IImplProvider, ICommand>(SaveAuctionCreateSession);

        public int Order => 1;

        public static void LoadAuctionCreateSessionCommandMembers(params string[] assemblyNames)
        {
            var commandMembers = assemblyNames.Select(s => Assembly.Load(s))
                .Select(assembly =>
                    assembly.GetTypes()
                        .Where(type => type.GetInterfaces().Contains(typeof(ICommand)))
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