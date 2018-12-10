namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using DataScheduledTask = ParkingRota.Data.ScheduledTask;
    using ModelScheduledTask = ParkingRota.Business.Model.ScheduledTask;

    public class ScheduledTaskRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> contextOptions;

        public ScheduledTaskRepositoryTests() =>
            this.contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Fact]
        public void Test_GetScheduledTasks()
        {
            // Arrange
            var scheduledTasks = new[]
            {
                new DataScheduledTask
                {
                    ScheduledTaskType = ScheduledTaskType.ReservationReminder,
                    NextRunTime = 10.December(2018).At(20, 0, 0).Utc()
                },
                new DataScheduledTask
                {
                    ScheduledTaskType = ScheduledTaskType.RequestReminder,
                    NextRunTime = 10.December(2018).At(21, 0, 0).Utc()
                }
            };

            this.SeedDatabase(scheduledTasks);

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.CreateMap<DataScheduledTask, ModelScheduledTask>();
            });

            using (var context = this.CreateContext())
            {
                // Act
                var result = new ScheduledTaskRepository(context, new Mapper(mapperConfiguration)).GetScheduledTasks();

                // Assert
                Assert.Equal(scheduledTasks.Length, result.Count);

                foreach (var scheduledTask in scheduledTasks)
                {
                    Assert.Single(result.Where(t =>
                        t.ScheduledTaskType == scheduledTask.ScheduledTaskType &&
                        t.NextRunTime == scheduledTask.NextRunTime));
                }
            }
        }

        [Fact]
        public void Test_UpdateScheduledTask()
        {
            // Arrange
            var reservationReminder = new DataScheduledTask
            {
                ScheduledTaskType = ScheduledTaskType.ReservationReminder,
                NextRunTime = 10.December(2018).At(20, 0, 0).Utc()
            };

            var requestReminder = new DataScheduledTask
            {
                ScheduledTaskType = ScheduledTaskType.RequestReminder,
                NextRunTime = 10.December(2018).At(21, 0, 0).Utc()
            };

            var scheduledTasks = new[] { reservationReminder, requestReminder };

            this.SeedDatabase(scheduledTasks);

            var updatedReservationReminder = new ModelScheduledTask
            {
                ScheduledTaskType = reservationReminder.ScheduledTaskType,
                NextRunTime = requestReminder.NextRunTime.Plus(1.Minutes())
            };

            // Act
            using (var context = this.CreateContext())
            {
                new ScheduledTaskRepository(context, Mock.Of<IMapper>()).UpdateScheduledTask(updatedReservationReminder);
            }

            // Assert
            using (var context = this.CreateContext())
            {
                var result = context.ScheduledTasks.ToArray();

                Assert.Equal(scheduledTasks.Length, result.Length);

                Assert.Single(result.Where(t =>
                    t.ScheduledTaskType == updatedReservationReminder.ScheduledTaskType &&
                    t.NextRunTime == updatedReservationReminder.NextRunTime));

                Assert.Single(result.Where(t =>
                    t.ScheduledTaskType == requestReminder.ScheduledTaskType &&
                    t.NextRunTime == requestReminder.NextRunTime));
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

        private void SeedDatabase(IReadOnlyList<DataScheduledTask> scheduledTasks)
        {
            using (var context = this.CreateContext())
            {
                context.ScheduledTasks.AddRange(scheduledTasks);
                context.SaveChanges();
            }
        }
    }
}