using Camping_retake.Models;
using LiteDB;
using System;

namespace Camping_retake.Data
{
    public class LiteDbContext : IDisposable
    {
        private readonly LiteDatabase _database;

        public LiteDbContext(string databasePath = "CampingDB_Retake.db")
        {
            _database = new LiteDatabase(databasePath);
        }

        // Collections
        public ILiteCollection<User> Users => _database.GetCollection<User>("Users");
        public ILiteCollection<Owner> Owners => _database.GetCollection<Owner>("Owners");
        public ILiteCollection<CampingSpot> CampingSpots => _database.GetCollection<CampingSpot>("CampingSpots");
        public ILiteCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");

        // Generate New ID
        private int GenerateNewId<T>(ILiteCollection<T> collection) where T : class
        {
            var maxId = collection
                .FindAll()
                .Select(entity => (int)entity.GetType().GetProperty("Id").GetValue(entity))
                .DefaultIfEmpty(0)
                .Max();

            return maxId + 1;
        }

        // User Methods
        public void AddUser(User user)
        {
            user.Id = GenerateNewId(Users);
            Users.Insert(user);
        }

        public IEnumerable<User> GetUsers()
        {
            return Users.FindAll();
        }

        public User GetUserById(int id)
        {
            return Users.FindById(id);
        }

        public void UpdateUser(User user)
        {
            Users.Update(user);
        }

        
        // Booking Methods
        public void AddBooking(Booking booking)
        {
            booking.Id = GenerateNewId(Bookings);
            Bookings.Insert(booking);
        }

        public IEnumerable<Booking> GetBookings()
        {
            return Bookings.FindAll();
        }

        public Booking GetBookingById(int id)
        {
            return Bookings.FindById(id);
        }

        public void UpdateBooking(Booking booking)
        {
            Bookings.Update(booking);
        }

        public bool DeleteBooking(int id)
        {
            return Bookings.Delete(id);
        }

        // CampingSpot Methods
        public void AddCampingSpot(CampingSpot campingSpot)
        {
            campingSpot.Id = GenerateNewId(CampingSpots);
            CampingSpots.Insert(campingSpot);
        }

        public IEnumerable<CampingSpot> GetCampingSpots()
        {
            return CampingSpots.FindAll();
        }

        public CampingSpot GetCampingSpotById(int id)
        {
            return CampingSpots.FindById(id);
        }

        public void UpdateCampingSpot(CampingSpot campingSpot)
        {
            CampingSpots.Update(campingSpot);
        }

        public bool DeleteCampingSpot(int id)
        {
            return CampingSpots.Delete(id);
        }

        // Owner Methods
        public void AddOwner(Owner owner)
        {
            owner.Id = GenerateNewId(Owners);
            Owners.Insert(owner);
        }

        public IEnumerable<Owner> GetOwners()
        {
            return Owners.FindAll();
        }

        public Owner GetOwnerById(int id)
        {
            return Owners.FindById(id);
        }

        public void UpdateOwner(Owner owner)
        {
            Owners.Update(owner);
        }

        public bool DeleteOwner(int id)
        {
            return Owners.Delete(id);
        }

        public LiteDatabase Database => _database;

        // Implement IDisposable
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _database?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
