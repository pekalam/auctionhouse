using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Core.Common.Exceptions;
using Core.Common.Query;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Core.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AuthorizationRequiredAttribute : Attribute, ICommandAttribute, IQueryAttribute
    {
        internal static Dictionary<Type, PropertyInfo> _signedInUserCommandProperties;
        internal static Dictionary<Type, PropertyInfo> _signedInUserQueryProperties;

        internal static void LoadSignedInUserCmdAndQueryMembers(params string[] assemblyNames)
        {
            var commandMembers = assemblyNames.Select(s => Assembly.Load((string) s))
                .Select(assembly =>
                    assembly.GetTypes()
                        .Where(type => type.BaseType == typeof(ICommand) || type.GetInterfaces().Contains(typeof(IQuery)))
                        .Select(type => type.GetProperties())
                        .SelectMany(infos => infos)
                        .Where(info =>
                            info.CanWrite &&
                            info.GetCustomAttribute<SignedInUserAttribute>() != null &&
                            info.PropertyType.Equals(typeof(UserId)))
                        .Select(info => new
                        {
                            CmdOrQueryType = info.DeclaringType,
                            PropertyInfo = info
                        })
                )
                .SelectMany(enumerable => enumerable);

            _signedInUserCommandProperties = new Dictionary<Type, PropertyInfo>();
            _signedInUserQueryProperties = new Dictionary<Type, PropertyInfo>();

            foreach (var member in commandMembers)
            {
                var interfaces = member.CmdOrQueryType.GetInterfaces();

                if (member.CmdOrQueryType.BaseType == typeof(ICommand) && !interfaces.Contains(typeof(IQuery)))
                {
                    _signedInUserCommandProperties[member.CmdOrQueryType] = member.PropertyInfo;
                }
                else if (interfaces.Contains(typeof(IQuery)) && member.CmdOrQueryType.BaseType != typeof(ICommand))
                {
                    _signedInUserQueryProperties[member.CmdOrQueryType] = member.PropertyInfo;
                }
                else
                {
                    throw new ArgumentException($"Invalid cmd/query of type: {member.CmdOrQueryType.FullName}");
                }

            }
        }

        public Action<IImplProvider, ICommand> PreHandleAttributeStrategy { get; } = new Action<IImplProvider, ICommand>(CheckCmdIsAuthorized);
        public Action<IImplProvider, ICommand> PostHandleAttributeStrategy { get; }
        Action<IImplProvider, IQuery> IQueryAttribute.AttributeStrategy { get; } = new Action<IImplProvider, IQuery>(CheckQueryIsAuthorized);

        public int Order => 0;

        private static UserId GetSignedInUser(IImplProvider implProvider)
        {
            var userIdentityService = implProvider.Get<IUserIdentityService>();
            var userIdentity = userIdentityService.GetSignedInUserIdentity();
            if (userIdentity == null || userIdentity == UserId.Empty)
            {
                throw new NotSignedInException("User not signed in");
            }

            return userIdentity;
        }

        private static void CheckCmdIsAuthorized(IImplProvider implProvider, ICommand commandBase)
        {
            var userIdentity = GetSignedInUser(implProvider);
            if (_signedInUserCommandProperties.ContainsKey(commandBase.GetType()))
            {
                _signedInUserCommandProperties[commandBase.GetType()].SetValue(commandBase, userIdentity);
            }
        }

        private static void CheckQueryIsAuthorized(IImplProvider implProvider, IQuery query)
        {
            var userIdentity = GetSignedInUser(implProvider);
            if (_signedInUserQueryProperties.ContainsKey(query.GetType()))
            {
                _signedInUserQueryProperties[query.GetType()].SetValue(query, userIdentity);
            }
        }
    }
}