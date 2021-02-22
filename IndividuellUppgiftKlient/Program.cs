using System;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using System.Text.Unicode;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using IndividuellUppgiftKlient;
using IndividuellUppgiftKlient.Models;
using IndividuellUppgiftKlient.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using IndividuellUppgiftKlient.Models.Requests;
using IndividuellUppgiftKlient.Models.Responses;
using System.Security.Cryptography.X509Certificates;

namespace IndividuellUppgiftKlient
{

    public class Program
    {
        static HttpClient client = new HttpClient();
        static UserDbContext dbContext = new UserDbContext(); 
        static async Task Main(string[] args)
        {
            Program p = new Program();

            //Denna kan du köra första gången du startar programmet sen ta bort.
            //Den seedar i båda databaserna. Användarna får namnet av sin respektive roll.
            //await p.SeedMethod();
            //await p.Login("Admin", "AsheTrash1!");
            await p.GetAllOrders("Admin");

        }

        //Users
        public async Task UpdateUser(string sender, string update)
        {
            var userToUpdate = await dbContext.users.Where(un => un.UserName == update).FirstOrDefaultAsync();
            var requestSender = await dbContext.users.Where(un => un.UserName == sender).FirstOrDefaultAsync();
            if (AuthorizeToken(requestSender))
            {
                //Ta bort kommentar på de fält du vill ändra och sätt önskat värde
                var updateValues = new UpdateUserRequest()
                {
                    Role = update
                };
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestSender.JwtToken);
                var request = await client.PutAsJsonAsync("https://localhost:5001/api/user/update" +"/"+ userToUpdate.UserName, updateValues);
                if(request != null && request.IsSuccessStatusCode)
                {
                    var serialized = await request.Content.ReadAsStringAsync();
                    var deserialized = DeSerialize(serialized);
                    userToUpdate.UserName = deserialized.UserName;
                    Console.WriteLine($"User {userToUpdate.UserName} has been updated");
                    await dbContext.SaveChangesAsync();
                    return;
                }
                Console.WriteLine("Unauthorized request");
                return;
            }
            Console.WriteLine("Users token is invalid.");
            return;
        }

        public async Task RegisterUser()
        {
            var registerUser = new RegisterUserRequest()
            {
                //Ta bort kommentarer och fyll i önskat värde. En ny användare kommer tillhöra 
                //rollen employee. För att ge användaren en annan roll måste admin skicka en updaterequest för användaren

                //UserName = 
                //Email = 
                //Password = 
                //EmpId = 
            };

            var request = await client.PostAsJsonAsync("https://localhost:5001/api/authentication/register", registerUser);
            if(request != null && request.IsSuccessStatusCode)
            {
                var response = await request.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };
                var receivedUser = JsonConvert.DeserializeObject<AuthResponse>(response, settings);
                var user = new User()
                {
                    UserName = receivedUser.UserName
                };
                dbContext.users.Add(user);
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"User {user.UserName} has now been registered");
                return;
            }
            Console.WriteLine("Request exception");
            return;

        }

        public async Task GetUser(string userName, string sender)
        {
            //Här kan du välja vem du vill hämta och vem du vill ska skicka requesten
            var userToGet = await dbContext.users.Where(u => u.UserName == userName).FirstOrDefaultAsync();
            var requestSender = await dbContext.users.Where(u => u.UserName == sender).FirstOrDefaultAsync();

            if (AuthorizeToken(requestSender))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestSender.JwtToken);
                var request = await client.GetAsync("https://localhost:5001/api/user/" + userToGet.UserName);
                if(request != null && request.IsSuccessStatusCode)
                {
                    var serialized = await request.Content.ReadAsStringAsync();
                    var response = DeSerialize(serialized); 
                    var user = new UserResponse()
                    {
                        UserName = response.UserName,
                        FirstName = response.FirstName,
                        LastName = response.LastName,
                        Country = response.Country,
                        Email = response.Email,
                    };
                    Console.WriteLine($"User name: {user.UserName}\nFirst name: {user.FirstName}\nLast name: {user.LastName}\nCountry: {user.Country}\nEmail: {user.Email}");
                    return;
                }
                Console.WriteLine("Unauthorized request");
                return;
            }
            Console.WriteLine("User Token has expired");
            return;
        }

        public async Task GetAllUsers(string sender)
        {
            var requestSender = await dbContext.users.Where(u => u.UserName == sender).FirstOrDefaultAsync();

            if (AuthorizeToken(requestSender))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestSender.JwtToken);
                var request = await client.GetAsync("https://localhost:5001/api/user/getall");
                
                if (request != null && request.IsSuccessStatusCode)
                {
                    var response = await request.Content.ReadAsStringAsync();
                    ParseJson(response);
                    return;
                }
                Console.WriteLine("Unauthorized request");
                return;
            }
            Console.WriteLine("Users token is not valid");
            return;
        }

        public async Task Login(string userName, string password)
        {
            //Ändra username för vem du vill logga in med. Samtliga användare har password
            //"AsheTrash1!"
            UserLogIn user = new UserLogIn()
            {
                UserName = userName,
                Password = password
            };
            var request = await client.PostAsJsonAsync("https://localhost:5001/api/authentication/login", user);
            if (request != null && request.IsSuccessStatusCode)
            {
                //Vid lyckad inloggning blir användaren tilldelad jwt och refreshtoken som sparas i db
                var response = await request.Content.ReadAsStringAsync();
                var deserializedResponse = DeSerialize(response);
                var loggedInUser = await dbContext.users.Where(u => u.UserName == user.UserName).FirstAsync();
                loggedInUser.JwtToken = deserializedResponse.JwtToken;
                loggedInUser.RefreshToken = deserializedResponse.RefreshToken;
                loggedInUser.RefreshExpires = deserializedResponse.RefreshExpires;
                loggedInUser.JwtExpires = deserializedResponse.JwtExpires;
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"User {loggedInUser.UserName} has successfully been logged in.");
                return;
            }
            Console.WriteLine("Password and/or username doest not match");
            return;
        }

        public async Task RefreshToken(string userName)
        {
            //Välj vilken användare du vill refresha. Endast användare som har loggat in har blivit tilldelade
            //tokens
            var user = await dbContext.users.Where(n => n.UserName == userName).FirstOrDefaultAsync();
            if(user== null || user.RefreshToken == null)
            {
                Console.WriteLine("User does not exist or does not have a valid refreshtoken");
                return;
            }

            var request = await client.PutAsJsonAsync($"https://localhost:5001/api/authentication/token/refreshtoken/" + user.RefreshToken, user);
            if (request != null && request.IsSuccessStatusCode)
            {
                //användaren blir tilldelad nya tokens
                var response = await request.Content.ReadAsStringAsync();
                var deserializedResponse = DeSerialize(response);
                user.JwtToken = deserializedResponse.JwtToken;
                user.JwtExpires = deserializedResponse.JwtExpires;
                user.RefreshToken = deserializedResponse.RefreshToken;
                user.RefreshExpires = deserializedResponse.RefreshExpires;
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"User {user.UserName}'s token has been refreshed");
                return;
            }
            Console.WriteLine("Unauthorized request " + request.RequestMessage);
        }

        //Orders
        public async Task GetMyOrders(string sender)
        {
            var requestSender = await dbContext.users.Where(u => u.UserName == sender).FirstOrDefaultAsync();

            //Om du skickar med admin eller CEO kan du välja att fylla i denna sträng.
            //Den tittar efter förnamn och efternamn i employees i northwind och returnerar ordrar för denne
            string usersOrders = "Dodsworth";
            if (AuthorizeToken(requestSender))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestSender.JwtToken);
                var request = await client.GetAsync($"https://localhost:5001/api/orders/getmyorders" + "/"+usersOrders);
                if (request != null && request.IsSuccessStatusCode)
                {
                    var response = await request.Content.ReadAsStringAsync();
                    ParseJson(response);
                    return;
                }
            }
            Console.WriteLine("User token is invalid");
            return;
        }

        public async Task GetCountryOrders(string sender, string countryName = null)
        {
            var requestSender = await dbContext.users.Where(u => u.UserName == sender).FirstOrDefaultAsync();
            //Om du skickar med admin eller ceo, fyll i denna sträng. Om du skickar med countrymanager kan du lämna den tom
            string country = countryName;
            if (AuthorizeToken(requestSender))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestSender.JwtToken);
                var request = await client.GetAsync($"https://localhost:5001/api/orders/country" + "/" + country);
                if(request != null && request.IsSuccessStatusCode)
                {
                    var response = await request.Content.ReadAsStringAsync();
                    ParseJson(response);
                    return;
                }
                Console.WriteLine("User is not authorized");
                return;
            }
            Console.WriteLine("Users token is invalid. ");
            return;
            
            
        }

        public async Task GetAllOrders(string sender)
        {
            //Välj vem som ska skicka requesten
            var requestSender = await dbContext.users.Where(u => u.UserName == sender).FirstOrDefaultAsync();
            if (AuthorizeToken(requestSender))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", requestSender.JwtToken);
                var request = await client.GetAsync($"https://localhost:5001/api/orders/getallorders");
                if(request != null && request.IsSuccessStatusCode)
                {
                    var response = await request.Content.ReadAsStringAsync();
                    ParseJson(response);
                    return;
                }
                Console.WriteLine("User is not authorized");
                return;
            }
            Console.WriteLine("Users token is invalid");
        }

        public bool AuthorizeToken(User user)
        {
            if (user.JwtIsExpired)
            {
                Console.WriteLine("User jwt is expired.");
                return false;
            }
            if (user.RefreshIsExpired)
            {
                Console.WriteLine("User refreshtoken is expired");
                return false;
            }
            return true;  
        }
        public AuthResponse DeSerialize(string response)
        {
            var settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
            var jsonDeserialized = JsonConvert.DeserializeObject<AuthResponse>(response, settings);
            return jsonDeserialized;
        }
        public void ParseJson(string content)
        {
            string[] parsed = content.Split(",");
            foreach(var i in parsed)
            {
                Console.WriteLine(i);
            }
        }

        public async Task SeedMethod()
        {
            var user1 = new RegisterUserRequest()
            {
                UserName = "Admin",
                Password = "AsheTrash1!",
                Email = "Ad@min.world",
                EmpId = 1
            };
            var user2 = new RegisterUserRequest()
            {
                UserName = "CEO",
                Password = "AsheTrash1!",
                Email = "CE@O.World",
                EmpId = 2
            };
            var user3 = new RegisterUserRequest()
            {
                UserName = "CountryManager",
                Password = "AsheTrash1!",
                Email = "Country@Manager.World",
                EmpId = 3
            };
            var user4 = new RegisterUserRequest()
            {
                UserName = "Employee",
                Password = "AsheTrash1!",
                Email = "Emp@Loyee.World",
                EmpId = 4
            };
            var request1 = await client.PostAsJsonAsync("https://localhost:5001/api/authentication/register", user1);
            var response1 = await request1.Content.ReadAsStringAsync();
            var receivedUser1 = JsonConvert.DeserializeObject<AuthResponse>(response1);
            var newUser1 = new User()
            {
                UserName = user1.UserName

            };
            var request2 = await client.PostAsJsonAsync("https://localhost:5001/api/authentication/register", user2);
            var response2 = await request2.Content.ReadAsStringAsync();
            var receivedUser2 = JsonConvert.DeserializeObject<AuthResponse>(response1);
            var newUser2 = new User()
            {
                UserName = user2.UserName

            };
            var request3 = await client.PostAsJsonAsync("https://localhost:5001/api/authentication/register", user3);
            var response3 = await request3.Content.ReadAsStringAsync();
            var receivedUser3 = JsonConvert.DeserializeObject<AuthResponse>(response1);
            var newUser3 = new User()
            {
                UserName = user3.UserName

            };
            var request4 = await client.PostAsJsonAsync("https://localhost:5001/api/authentication/register", user4);
            var response4 = await request4.Content.ReadAsStringAsync();
            var receivedUser4 = JsonConvert.DeserializeObject<AuthResponse>(response1);
            var newUser4 = new User()
            {
                UserName = user4.UserName

            };
            dbContext.users.Add(newUser1);
            dbContext.users.Add(newUser2);
            dbContext.users.Add(newUser3);
            dbContext.users.Add(newUser4);
            dbContext.SaveChanges();

            Program p = new Program();
            await p.Login("Admin", "AsheTrash1!");
            await p.UpdateUser("Admin", "CountryManager");
            await p.UpdateUser("Admin", "CEO");


        }
        
    }
}

