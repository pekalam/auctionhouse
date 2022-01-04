using Common.Application.Queries;
using System.Reflection;

namespace Common.Application.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AuthorizationRequiredAttribute : Attribute, ICommandAttribute, IQueryAttribute
    {
        internal static Dictionary<Type, PropertyInfo> _signedInUserCommandProperties;
        internal static Dictionary<Type, PropertyInfo> _signedInUserQueryProperties;

        public static void LoadSignedInUserCmdAndQueryMembers(params string[] assemblyNames)
        {
            var commandMembers = assemblyNames.Select(s => Assembly.Load(s))
                .Select(assembly =>
                    assembly.GetTypes()
                        .Where(type => type.GetInterfaces().Contains(typeof(ICommand)) || type.GetInterfaces().Contains(typeof(IQuery)))
                        .Select(type => type.GetProperties())
                        .SelectMany(infos => infos)
                        .Where(info =>
                            info.CanWrite &&
                            info.GetCustomAttribute<SignedInUserAttribute>() != null &&
                            info.PropertyType.Equals(typeof(Guid)))
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

                if (interfaces.Contains(typeof(ICommand)) && !interfaces.Contains(typeof(IQuery)))
                {
                    _signedInUserCommandProperties[member.CmdOrQueryType] = member.PropertyInfo;
                }
                else if (interfaces.Contains(typeof(IQuery)) && !interfaces.Contains(typeof(ICommand)))
                {
                    _signedInUserQueryProperties[member.CmdOrQueryType] = member.PropertyInfo;
                }
                else
                {
                    throw new ArgumentException($"Invalid cmd/query of type: {member.CmdOrQueryType.FullName}");
                }

            }
        }

        public Action<IImplProvider, CommandContext, ICommand> PreHandleAttributeStrategy { get; } = new Action<IImplProvider, CommandContext, ICommand>(CheckCmdIsAuthorized);
        public Action<IImplProvider, CommandContext, ICommand> PostHandleAttributeStrategy { get; }
        Action<IImplProvider, CommandContext, IQuery> IQueryAttribute.AttributeStrategy { get; } = new Action<IImplProvider, CommandContext, IQuery>(CheckQueryIsAuthorized);

        public int Order => 0;

        private static Guid GetSignedInUser(IImplProvider implProvider)
        {
            var userIdentityService = implProvider.Get<IUserIdentityService>();
            var userIdentity = userIdentityService.GetSignedInUserIdentity();
            if (userIdentity == Guid.Empty)
            {
                throw new InvalidOperationException($"{nameof(IUserIdentityService)} returned empty user id");
            }

            return userIdentity;
        }

        private static void CheckCmdIsAuthorized(IImplProvider implProvider, CommandContext ctx, ICommand commandBase)
        {
            if (_signedInUserCommandProperties.ContainsKey(commandBase.GetType()))
            {
                var userIdentity = GetSignedInUser(implProvider);
                ctx.User = userIdentity;
                _signedInUserCommandProperties[commandBase.GetType()].SetValue(commandBase, userIdentity);
            }
        }

        private static void CheckQueryIsAuthorized(IImplProvider implProvider, CommandContext ctx, IQuery query)
        {
            if (_signedInUserQueryProperties.ContainsKey(query.GetType()))
            {
                var userIdentity = GetSignedInUser(implProvider);
                ctx.User = userIdentity;
                _signedInUserQueryProperties[query.GetType()].SetValue(query, userIdentity);
            }
        }
    }
}