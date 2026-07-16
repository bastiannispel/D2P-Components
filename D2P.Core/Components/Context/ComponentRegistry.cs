using System;

using System.Collections.Generic;

using System.Linq;

using System.Reflection;



using D2P.Core.Exceptions;

using D2P.Core.Interfaces;



namespace D2P.Core.Components {

    public sealed class ComponentRegistry {

        readonly Dictionary<string, TypeRegistration> _registrations = new Dictionary<string, TypeRegistration>(StringComparer.Ordinal);



        public IEnumerable<string> TypeIds => _registrations.Keys;

        public IEnumerable<TypeRegistration> Registrations => _registrations.Values;



        public void Register<T>(string typeId) where T : class, IComponentBase

        {

            Register(typeof(T), new TypeRegistration(typeof(T), typeId));

        }


        public void RegisterFromCallingAssembly()

        {

            RegisterFromAssembly(Assembly.GetCallingAssembly());

        }



        public void RegisterFromAssembly(Assembly assembly)

        {

            foreach (var type in GetComponentTypes(assembly)) {

                var attribute = type.GetCustomAttribute<ComponentTypeAttribute>(inherit: false);

                if (attribute == null)

                    throw new ComponentTypeAttributeMissingException(type);



                Register(type, new TypeRegistration(type, attribute));

            }

        }



        public void Register(TypeRegistration registration)

        {

            Register(registration.ClrType, registration);

        }



        public bool TryResolve(string typeId, out TypeRegistration registration)
        {
            return _registrations.TryGetValue(typeId, out registration);
        }



        public bool TryResolveType<T>(out string typeId) where T : class, IComponentBase

        {

            var match = _registrations.FirstOrDefault(entry => entry.Value.ClrType == typeof(T));

            if (match.Equals(default(KeyValuePair<string, TypeRegistration>))) {

                typeId = string.Empty;

                return false;

            }



            typeId = match.Key;

            return true;

        }



        void Register(Type clrType, TypeRegistration registration)

        {

            if (_registrations.TryGetValue(registration.TypeId, out var existing)) {

                if (existing.ClrType == clrType)

                    return;

                throw new TypeIdAlreadyRegisteredException(registration.TypeId, existing.ClrType, clrType);

            }



            _registrations.Add(registration.TypeId, registration);

        }



        static IEnumerable<Type> GetComponentTypes(Assembly assembly)

        {

            return assembly.GetTypes()

                .Where(type => typeof(ComponentBase).IsAssignableFrom(type))

                .Where(type => !type.IsAbstract)

                .Where(type => !IsExcludedFromScan(type));

        }



        static bool IsExcludedFromScan(Type type)

        {

            if (type == typeof(Component))

                return true;

            if (type == typeof(Member.Member))

                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Member.Member<>))

                return true;

            if (type == typeof(Member.MemberCollection))

                return true;

            return false;

        }

    }

}

