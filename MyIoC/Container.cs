using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC
{
	public class Container
	{
        private readonly Dictionary<Type, Type> _registeredTypes = new Dictionary<Type, Type>();

        private readonly Assembly _assembly;

        public Container()
        {
            _assembly = Assembly.GetExecutingAssembly();
            AddAssembly(_assembly);
        }

        public Container(Assembly assembly)
        {
            _assembly = assembly;
            AddAssembly(_assembly);
        }

        public void AddAssembly(Assembly assembly)
        { 
            List<Type> typesToImport = new List<Type>();

            var importTypes = assembly.GetTypes().SelectMany(type => type.GetProperties())
                .Where(prop => prop.GetCustomAttribute<ImportAttribute>() != null).Select(prop => prop.PropertyType);

            var constuctorArgs = assembly.GetTypes().Where(type => type.IsClass && type.GetCustomAttribute<ImportConstructorAttribute>() != null)
                .SelectMany(type => type.GetConstructors()).OrderByDescending(c => c.GetParameters().Length)
                .First().GetParameters().Select(param => param.ParameterType);

            typesToImport.AddRange(importTypes);
            typesToImport.AddRange(constuctorArgs);

            var exportTypes = assembly.GetTypes().Where(type => type.IsClass && type.GetCustomAttribute<ExportAttribute>() != null);

            foreach (var impType in typesToImport)
            {
                foreach (var expType in exportTypes)
                {
                    if (impType == expType || impType == expType.GetCustomAttribute<ExportAttribute>().Contract)
                    {
                        AddType(expType, impType);
                    }
                }
            }
        }

		public void AddType(Type type)
		{
            if (!_registeredTypes.ContainsKey(type))
                _registeredTypes.Add(type, type);
        }

		public void AddType(Type type, Type baseType)
		{
            if (!_registeredTypes.ContainsKey(baseType))
                _registeredTypes.Add(baseType, type);
        }


		public object CreateInstance(Type type)
		{      
                if (_registeredTypes.ContainsKey(type))
                {
                    if (type != _registeredTypes[type])
                    {
                        return CreateInstance(_registeredTypes[type]);
                    }
                }
                var constructor = type.GetConstructors()
                   .OrderByDescending(c => c.GetParameters().Length)
                   .First();

                var args = constructor.GetParameters().Select(param =>
                    CreateInstance(param.ParameterType))
                    .ToArray();

                return Activator.CreateInstance(type, args);

        }

        public object ResolveInstanceProperties(Type type)
        {    
            var instance = Activator.CreateInstance(type);

            foreach (var prop in type.GetProperties())
            {
                if (prop.GetCustomAttribute<ImportAttribute>() != null)
                {
                     if (_registeredTypes.ContainsKey(prop.PropertyType))
                    {
                        prop.SetValue(instance, CreateInstance(_registeredTypes[prop.PropertyType]));
                    }
                    else
                    {
                        prop.SetValue(instance, CreateInstance(prop.PropertyType));
                    }
                }
            }
            return instance;
        }

        public T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        public T ResolveInstanceProperties<T>()
		{
            return (T)ResolveInstanceProperties(typeof(T));
        }


		public void Sample()
		{
			var container = new Container();
			container.AddAssembly(Assembly.GetExecutingAssembly());

			var customerBLL = (CustomerBLL)container.CreateInstance(typeof(CustomerBLL));
			var customerBLL2 = container.CreateInstance<CustomerBLL>();

			container.AddType(typeof(CustomerBLL));
			container.AddType(typeof(Logger));
			container.AddType(typeof(CustomerDAL), typeof(ICustomerDAL));
		}
	}
}
