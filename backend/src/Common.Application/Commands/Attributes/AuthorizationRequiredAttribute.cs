using Common.Application.Queries;
using System.Reflection;

namespace Common.Application.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AuthorizationRequiredAttribute : Attribute, ICommandAttribute, IQueryAttribute
    {
        internal static Dictionary<Type, PropertyInfo> _signedInUserCommandProperties;
        internal static Dictionary<Type, PropertyInfo> _signedInUserQueryProperties;
        internal static HashSet<Type> _commandsWithAttr;
        internal static HashSet<Type> _queriesWithAttr;

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
                .SelectMany(enumerable => enumerable).ToArray();

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

            _commandsWithAttr =new(assemblyNames.Select(s => Assembly.Load(s))
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(type => type.GetInterfaces().Contains(typeof(ICommand))
                            && type.GetCustomAttribute<AuthorizationRequiredAttribute>() != null
                    )));
            _queriesWithAttr = new(assemblyNames.Select(s => Assembly.Load(s))
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(type => type.GetInterfaces().Contains(typeof(IQuery))
                            && type.GetCustomAttribute<AuthorizationRequiredAttribute>() != null
                    )));
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
            var cmdType = commandBase.GetType();
            if (_commandsWithAttr.Contains(cmdType))
            {
                var userIdentity = GetSignedInUser(implProvider);
                ctx.User = userIdentity;
                if(_signedInUserCommandProperties.ContainsKey(cmdType))
                    _signedInUserCommandProperties[cmdType].SetValue(commandBase, userIdentity);
            }
        }

        private static void CheckQueryIsAuthorized(IImplProvider implProvider, CommandContext ctx, IQuery query)
        {
            var queryType = query.GetType();
            if (_queriesWithAttr.Contains(queryType))
            {
                var userIdentity = GetSignedInUser(implProvider);
                ctx.User = userIdentity;
                if(_signedInUserQueryProperties.ContainsKey(queryType))
                    _signedInUserQueryProperties[queryType].SetValue(query, userIdentity);
            }
        }
    }
}