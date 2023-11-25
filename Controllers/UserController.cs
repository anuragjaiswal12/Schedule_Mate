using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Schedule_Mate.Filters;
using Schedule_Mate.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Schedule_Mate.Controllers
{
    public class UserController : Controller
    {
        [HttpGet, LoggedInFilter]
        public ActionResult Login() => View();

        [HttpPost, LoggedInFilter]
        public ActionResult Login(CollectionUser user)
        {
            //Check if provided data is null then redirect
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            PasswordHasher Ph = new();
            string encryptedPassword = Ph.EncryptPassword(user.Password);
            user.Password = encryptedPassword;

            //Check if user credentials are correct then redirect to Index page
            CollectionUser findUser = new MongoDBServices().database.GetCollection<CollectionUser>("userCollection").Find(a => a.Email == user.Email && a.Password == user.Password).FirstOrDefault();
            if (findUser != null)
            {
                HttpContext.Session.SetString("User", findUser.Email);

                //Chcek if user has no Schedule than redirect to Add Schedule page
                IMongoCollection<CollectionSchedule> scheduleCollection = new MongoDBServices().database.GetCollection<CollectionSchedule>("scheduleCollection");
                int scheduleCount = scheduleCollection.Find(a => a.Email == user.Email).ToList().Count;
                if (scheduleCount == 0)
                {
                    return RedirectToAction("AddSchedule");
                }
                return RedirectToAction("Index");
            }
            ErrorMsgModel error = new()
            {
                ModalMsg = "Wrong email or password",
                ModalTitle = "Invalid credentials",
                Title = "Login Page",
                Url = "Login"
            };

            //if nothing returns that means above's any condition doesn't satisfied so return LoginError View
            return View("Modal", error);
        }

        [HttpGet, LoggedInFilter]
        public ActionResult Registration() => View();

        [HttpPost, LoggedInFilter]
        public ActionResult Registration(CollectionUser user)
        {
            //Creating collection refrence
            IMongoCollection<CollectionUser> userCollection = new MongoDBServices().database.GetCollection<CollectionUser>("userCollection");

            //Check if user alredy registered then return to RegistrationError page
            CollectionUser checkUser = new MongoDBServices().database.GetCollection<CollectionUser>("userCollection").Find(a => a.Email == user.Email).FirstOrDefault();
            if (checkUser != null)
            {
                ErrorMsgModel error = new()
                {
                    ModalMsg = "Email was already used for another account try diffrent email or try to login",
                    ModalTitle = "Invalid E-Mail",
                    Title = "Registration Page",
                    Url = "Registration"
                };
                return View("Modal", error);
            }

            //If user profile pic was not selected then set the default image
            if (user.ProfilePic == null)
            {
                user.ProfilePic = "default.png";
            }

            PasswordHasher Ph = new();
            string encryptedPassword = Ph.EncryptPassword(user.Password);
            user.Password = encryptedPassword;

            //If everything is ok than create user and send user to Login page
            userCollection.InsertOne(user);

            ErrorMsgModel success = new()
            {
                ModalMsg = "Registration successful, go to Login page",
                ModalTitle = "Congratulations",
                Title = "Registration Page",
                Url = "Login"
            };
            return View("Modal", success);
        }

        [LoginFilter]
        public ActionResult Index()
        {
            IMongoCollection<CollectionDailyTask> dailyTaskCollection = new MongoDBServices().database.GetCollection<CollectionDailyTask>("dailyTaskCollection");
            CollectionDailyTask todayTask = dailyTaskCollection.Find(a => a.Email == HttpContext.Session.GetString("User") && a.TaskDate == DateTime.Today).FirstOrDefault();
            return View(todayTask);
        }

        [HttpGet, LoginFilter]
        public ActionResult Schedule()
        {
            string? currentUser = HttpContext.Session.GetString("User");

            IMongoCollection<CollectionSchedule> scheduleCollection = new MongoDBServices().database.GetCollection<CollectionSchedule>("scheduleCollection");
            List<CollectionSchedule> schedules = scheduleCollection.Find(a => a.Email == currentUser).ToList();

            return View(schedules);
        }

        [HttpGet, LoginFilter]
        public ActionResult AddSchedule() => View();

        [HttpPost, LoginFilter]
        public ActionResult AddSchedule(CollectionSchedule schedule)
        {
            //Getting current user's E-Mail
            string? currentUser = HttpContext.Session.GetString("User");

            //Creating collection refrence
            IMongoCollection<CollectionSchedule> scheduleCollection = new MongoDBServices().database.GetCollection<CollectionSchedule>("scheduleCollection");

            //Chcek if there is no shedule for current user then set "Default" Schedule Name 
            int scheduleCount = scheduleCollection.Find(a => a.Email == currentUser).ToList().Count;
            if (scheduleCount == 0)
            {
                schedule.ScheduleName = "Default";
            }

            //Check if same name Schedule already exist than return ScheduleError page
            CollectionSchedule checkCollection = scheduleCollection.Find(a => a.Email == currentUser && a.ScheduleName == schedule.ScheduleName).FirstOrDefault();
            if (checkCollection != null)
            {
                ErrorMsgModel error = new()
                {
                    ModalMsg = "Please enter unique schedule name",
                    ModalTitle = "Duplicate Schedule Name",
                    Title = "Add Schedule",
                    Url = "AddSchedule"
                };
                return View("Modal", error);
            }

            //Generating unique Schedule id using guid and user email
            // int len = scheduleCollection.Find(a => a.Email == currentUser).ToList().Count;
            Guid id = Guid.NewGuid();
            string scheduleId = "S" + "_" + id + "_" + currentUser?.Split("@")[0];
            schedule.ScheduleId = scheduleId;

            schedule.Email = currentUser ?? "";

            scheduleCollection.InsertOne(schedule);

            ErrorMsgModel success = new()
            {
                ModalMsg = "Schedule added successfully",
                ModalTitle = "Success",
                Title = "Add Schedule",
                Url = "Index"
            };

            return View("Modal", success);
        }

        [LoginFilter, HttpPost]
        public ActionResult DeleteSchedule(string scheduleId)
        {
            IMongoCollection<CollectionSchedule> scheduleCollection = new MongoDBServices().database.GetCollection<CollectionSchedule>("scheduleCollection");
            scheduleCollection.FindOneAndDelete(a => a.ScheduleId == scheduleId);
            return Ok();
        }

        [LoginFilter, HttpPost]
        public ActionResult SelectSchedule(string scheduleId, DateTime Date)
        {
            string? currentUser = HttpContext.Session.GetString("User");
            IMongoCollection<CollectionSchedule> scheduleCollection = new MongoDBServices().database.GetCollection<CollectionSchedule>("scheduleCollection");
            IMongoCollection<CollectionDailyTask> dailyTaskCollection = new MongoDBServices().database.GetCollection<CollectionDailyTask>("dailyTaskCollection");
            CollectionSchedule schedule = scheduleCollection.Find(a => a.Email == currentUser && a.ScheduleId == scheduleId).FirstOrDefault();
            List<TaskModel>? taskList = JsonSerializer.Deserialize<List<TaskModel>>(schedule.TaskList);
            List<TaskDataModel> taskDataList = new List<TaskDataModel>();
            Date = Date.AddDays(1);
            foreach (TaskModel Task in taskList)
            {
                TaskDataModel newTaskData = new()
                {
                    taskName = Task.taskName,
                    duration = Task.duration,
                    status = "Pending"
                };
                taskDataList.Add(newTaskData);
            }

            string newTaskDataList = JsonSerializer.Serialize(taskDataList);

            CollectionDailyTask dailyTask = new()
            {
                Email = currentUser,
                ScheduleId = scheduleId,
                TaskDate = Date,
                TaskList = newTaskDataList,
                ScheduleName = schedule.ScheduleName
            };

            CollectionDailyTask existingTask = dailyTaskCollection.Find(a => a.TaskDate == Date && a.Email == currentUser).FirstOrDefault();

            //condition for checking and updating/inserting the daily task
            if (existingTask != null)
            {
                dailyTaskCollection.DeleteOne(a => a.TaskDate == Date && a.Email == currentUser);
            }

            dailyTaskCollection.InsertOne(dailyTask);

            return Ok();
        }

        [LoginFilter, HttpPost]
        public ActionResult UpdateScheduleName(string scheduleId, string scheduleName)
        {
            IMongoCollection<CollectionSchedule> scheduleCollection = new MongoDBServices().database.GetCollection<CollectionSchedule>("scheduleCollection");

            var update = Builders<CollectionSchedule>.Update.Set(a => a.ScheduleName, scheduleName);

            scheduleCollection.FindOneAndUpdate(a => a.ScheduleId == scheduleId, update);

            return Ok();
        }

        [LoginFilter, HttpPost]
        public ActionResult DeleteTask(string scheduleId, int index)
        {
            IMongoCollection<CollectionSchedule> scheduleCollection = new MongoDBServices().database.GetCollection<CollectionSchedule>("scheduleCollection");

            CollectionSchedule schedule = scheduleCollection.Find(a => a.Email == HttpContext.Session.GetString("User") && a.ScheduleId == scheduleId).FirstOrDefault();
            List<TaskModel> taskList = JsonSerializer.Deserialize<List<TaskModel>>(schedule.TaskList);
            taskList.RemoveAt(index);
            string newTaskList = JsonSerializer.Serialize(taskList);
            var update = Builders<CollectionSchedule>.Update.Set(a => a.TaskList, newTaskList);
            scheduleCollection.FindOneAndUpdate(a => a.ScheduleId == scheduleId && a.Email == HttpContext.Session.GetString("User"), update);
            return Ok();
        }

        [LoginFilter, HttpPost]
        public ActionResult UpdateTask(string ScheduleId, int Index, string TaskName, string Duration)
        {
            IMongoCollection<CollectionSchedule> scheduleCollection = new MongoDBServices().database.GetCollection<CollectionSchedule>("scheduleCollection");

            CollectionSchedule schedule = scheduleCollection.Find(a => a.Email == HttpContext.Session.GetString("User") && a.ScheduleId == ScheduleId).FirstOrDefault();
            List<TaskModel> taskList = JsonSerializer.Deserialize<List<TaskModel>>(schedule.TaskList);
            taskList[Index] = new TaskModel
            {
                taskName = TaskName,
                duration = Duration,
            };

            string newTaskList = JsonSerializer.Serialize(taskList);
            var update = Builders<CollectionSchedule>.Update.Set(a => a.TaskList, newTaskList);
            scheduleCollection.FindOneAndUpdate(a => a.ScheduleId == ScheduleId && a.Email == HttpContext.Session.GetString("User"), update);
            return Ok();
        }

        [LoginFilter, HttpPost]
        public ActionResult AddTask(string ScheduleId, string TaskName, string Duration)
        {
            IMongoCollection<CollectionSchedule> scheduleCollection = new MongoDBServices().database.GetCollection<CollectionSchedule>("scheduleCollection");

            CollectionSchedule schedule = scheduleCollection.Find(a => a.Email == HttpContext.Session.GetString("User") && a.ScheduleId == ScheduleId).FirstOrDefault();
            List<TaskModel> taskList = JsonSerializer.Deserialize<List<TaskModel>>(schedule.TaskList);

            TaskModel newTask = new()
            {
                taskName = TaskName,
                duration = Duration,
            };
            taskList.Add(newTask);
            string newTaskList = JsonSerializer.Serialize(taskList);
            var update = Builders<CollectionSchedule>.Update.Set(a => a.TaskList, newTaskList);
            scheduleCollection.FindOneAndUpdate(a => a.ScheduleId == ScheduleId && a.Email == HttpContext.Session.GetString("User"), update);
            return Ok();
        }

        [LoginFilter, HttpGet]
        public ActionResult Profile()
        {
            CollectionUser currentUser = new MongoDBServices().database.GetCollection<CollectionUser>("userCollection").Find(a => a.Email == HttpContext.Session.GetString("User")).FirstOrDefault();

            return View(currentUser);
        }

        [LoginFilter, HttpPost]
        public ActionResult UpdateProfile(CollectionUser user)
        {
            if (user.Name == null || user.Name.Length < 2)
            {
                ErrorMsgModel error = new()
                {
                    ModalMsg = "Please enter a valid name",
                    ModalTitle = "Invalid name",
                    Title = "Profile",
                    Url = "Profile"
                };
                return View("Modal", error);
            }
            IMongoCollection<CollectionUser> userCollection = new MongoDBServices().database.GetCollection<CollectionUser>("userCollection");

            var update = Builders<CollectionUser>.Update.Set(a => a.Name, user.Name).Set(a => a.ProfilePic, user.ProfilePic);

            userCollection.FindOneAndUpdate(a => a.Email == HttpContext.Session.GetString("User"), update);

            return RedirectToAction("Profile");
        }

        [LoginFilter, HttpPost]
        public ActionResult UpdatePassword(string OldPassword, string NewPaswword, string ConfirmPassword)
        {
            IMongoCollection<CollectionUser> userCollection = new MongoDBServices().database.GetCollection<CollectionUser>("userCollection");
            CollectionUser user = userCollection.Find(a => a.Email == HttpContext.Session.GetString("User")).FirstOrDefault();
            PasswordHasher ph = new();
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?:{}|<>]).{8,}$");
            string msg = "Password updated successfully!";
            string status = "success";
            string actualPassword = ph.DecryptPassword(user.Password);
            if (OldPassword != actualPassword)
            {
                msg = "Your old password was wrong";
                status = "failed";
            }
            else if (!regex.IsMatch(NewPaswword))
            {
                msg = "Password should contain minimum 8 characters, at least one uppercase letter, one lowercase letter, one special character, and one number";
                status = "failed";
            }
            else if (NewPaswword != ConfirmPassword)
            {
                msg = "New password and confirm password should be same";
                status = "failed";
            }
            else
            {
                string encryptedPassword = ph.EncryptPassword(NewPaswword);
                var update = Builders<CollectionUser>.Update.Set(a => a.Password, encryptedPassword);
                userCollection.FindOneAndUpdate(a => a.Email == HttpContext.Session.GetString("User"), update);
            }

            return Json(new { msg, status });
        }

        [LoginFilter, HttpGet]
        public ActionResult History()
        {
            IMongoCollection<CollectionDailyTask> dailyTaskCollection = new MongoDBServices().database.GetCollection<CollectionDailyTask>("dailyTaskCollection");
            List<CollectionDailyTask> taskHistory = dailyTaskCollection.Find(a => a.Email == HttpContext.Session.GetString("User") && a.TaskDate < DateTime.Today).Sort(Builders<CollectionDailyTask>.Sort.Descending(a => a.TaskDate)).ToList();
            return View(taskHistory);
        }

        [LoginFilter, HttpGet]
        public ActionResult Statstics()
        {
            IMongoCollection<CollectionDailyTask> dailyTaskCollection = new MongoDBServices().database.GetCollection<CollectionDailyTask>("dailyTaskCollection");
            List<CollectionDailyTask> taskHistory = dailyTaskCollection.Find(a => a.Email == HttpContext.Session.GetString("User") && a.TaskDate < DateTime.Today).Sort(Builders<CollectionDailyTask>.Sort.Ascending(a => a.TaskDate)).Limit(7).ToList();
            return View(taskHistory);
        }

        [LoginFilter, HttpPost]
        public ActionResult UpdateTaskStatus(int index, string status)
        {
            IMongoCollection<CollectionDailyTask> dailyTaskCollection = new MongoDBServices().database.GetCollection<CollectionDailyTask>("dailyTaskCollection");
            CollectionDailyTask schedule = dailyTaskCollection.Find(a => a.TaskDate == DateTime.Today && a.Email == HttpContext.Session.GetString("User")).FirstOrDefault();
            List<TaskDataModel> taskData = JsonSerializer.Deserialize<List<TaskDataModel>>(schedule.TaskList);
            taskData[index].status = status;

            var update = Builders<CollectionDailyTask>.Update.Set(a => a.TaskList, JsonSerializer.Serialize(taskData));
            dailyTaskCollection.FindOneAndUpdate(a => a.TaskDate == DateTime.Today && a.Email == HttpContext.Session.GetString("User"), update);
            return Ok();
        }

        [LoginFilter]
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}