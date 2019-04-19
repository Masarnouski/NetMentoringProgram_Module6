using MyIoC.Exceptions;
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
            var listOfTypes = assembly.GetTypes();
            foreach (var type in listOfTypes)
            {
                var typeImportConstrAttr = type.GetCustomAttribute<ImportConstructorAttribute>();
                var typeImportPropAttr = type.GetProperties().Where(x => x.GetCustomAttribute<ImportAttribute>() != null);

                if (typeImportConstrAttr != null || typeImportPropAttr.Count() > 0)
                {
                    AddType(type, type);
                }

                var typeExportAttr = type.GetCustomAttributes<ExportAttribute>();
                foreach (var exportAttr in typeExportAttr)
                {
                    if (exportAttr.Contract != null)
                    {
                        AddType(type, exportAttr.Contract);
                    }
                    else
                    {
                        AddType(type, type);
                    }
                }
            }
        }

        public void AddType(Type type)
        {
            if (!_registeredTypes.ContainsKey(type))
            {
                _registeredTypes.Add(type, type);
            }
        }

		public void AddType(Type type, Type baseType)
		{

            if (!_registeredTypes.ContainsKey(baseType))
            {
                _registeredTypes.Add(baseType, type);
            }
        }


        public object CreateInstance(Type type)
        {
            if (!_registeredTypes.ContainsKey(type))
            {
                throw new CustomIoCExpection($"CreateInstance method thrown an exception. Type {type.Name} is not registered.");
            }

            var typeToGetInstance = _registeredTypes[type];
            var constructors = typeToGetInstance.GetConstructors();
               

            if (constructors.Length == 0)
            {
                throw new CustomIoCExpection($"CreateInstance method thrown an exception. Type {type.Name} doesn't have a public constructor.");
            }

            var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();

            var args = constructor.GetParameters().Select(param =>
                CreateInstance(param.ParameterType))
                .ToArray();

            var instance = Activator.CreateInstance(typeToGetInstance, args);

            if (type.GetCustomAttribute<ImportConstructorAttribute>() != null)
            {
                return instance;
            }

            ResolveProperties(type, instance);
            return instance;

        }

        private void ResolveProperties(Type type, object instance)
        {    
            var propertiesToResolve = type.GetProperties().Where(prop =>prop.GetCustomAttribute<ImportAttribute>() != null);
            foreach (var prop in propertiesToResolve)
            {
                if (_registeredTypes.ContainsKey(prop.PropertyType))
                {
                    var resolvedProperty = CreateInstance(_registeredTypes[prop.PropertyType]);
                    prop.SetValue(instance, resolvedProperty);
                }
                else
                {
                    throw new CustomIoCExpection($"Can't resolve property {prop.Name}. Type {prop.PropertyType} is not registered.");
                }
            }
        }

        public T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
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
