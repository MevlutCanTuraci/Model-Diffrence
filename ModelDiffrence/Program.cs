using System.Reflection;

namespace ModelDiffrence
{
    public class Program
	{
        public static void Main()
        {
            var userSendData = new Person
            {
                Name        = "Mevlut Can",
                Surname     = "Turacı",
                Age         = 21,
                Email       = "example@example.com"
            };

            var databaseInData = new Person
            {
                Name = "Mevlüt",
                Surname = "Can",
                Age = 19,
                Email = "example@example.com"
            };

            var diffrencedModel = CompareAndReturnUpdatedModel<Person>(databaseInData, userSendData);
            Console.WriteLine($@"
Name:    {databaseInData.Name} to {diffrencedModel.Name}
Surname: {databaseInData.Surname} to {diffrencedModel.Surname}
Age:     {databaseInData.Age} to {diffrencedModel.Age}
Email:   {databaseInData.Email} to {diffrencedModel.Email}
            ");

            Console.Read();
        }


        #region Diffrence Function

        public static T CompareAndReturnUpdatedModel<T>(T model1, T model2) where T : class
        {
            T updatedModel = (T)Activator.CreateInstance(typeof(T));

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            foreach (var property in properties)
            {
                object value1 = property.GetValue(model1);
                object value2 = property.GetValue(model2);

                if (!object.Equals(value1, value2))
                {
                    property.SetValue(updatedModel, value2);
                }
                else
                {
                    property.SetValue(updatedModel, null);
                }
            }

            return updatedModel;
        }

        #endregion
    }

    public class Person
	{
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public int? Age { get; set; }
    }
}