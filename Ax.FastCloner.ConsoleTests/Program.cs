using System;
using System.Collections.Generic;

namespace Ax.FastCloner.ConsoleTests
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var cloner = new Cloner(new FieldClonerConfiguration());
            var person = new Person
            {
                Name = "Albert Einstein",
                Age = 120,
                Address = new Address
                {
                    Street = "4th St",
                    Number = "500 E"
                },

                PhoneNumbers = new string[] { "123456789", "987654321" } 
            };

            person.Self = person;

            var clonedPerson = cloner.Clone(person);

            var stringObj = "Test String";
            var clonedStringObj = cloner.Clone(stringObj);
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public string[] PhoneNumbers { get; set; }

        public Person Self { get; set; }
    }

    public class Address 
    {
        public string Street { get; set; }
        public string Number { get; set; }
    }
}
