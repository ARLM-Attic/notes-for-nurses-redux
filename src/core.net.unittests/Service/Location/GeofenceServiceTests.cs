﻿// -----------------------------------------------------------------------
// <copyright file="GeofenceServiceTests.cs">
// Copyright Edward Wilde (c) 2013.
// </copyright>
// -----------------------------------------------------------------------


using System;

using Machine.Fakes;
using Machine.Specifications;

namespace core.net.tests.Service
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Edward.Wilde.Note.For.Nurses.Core.Model;
    using Edward.Wilde.Note.For.Nurses.Core.Service;

    using core.net.tests.Service.Contexts;
    using core.net.tests.UI;
    using core.net.tests.UI.Behaviors;

    [Subject(typeof(GeofenceService))]
    public class when_the_geofenceservice_is_constructed : WithConcreteSubject<GeofenceService, IGeofenceService>
    {
        Establish context = () => With<NewGeofenceService>();

        Because of = () => { };

        Behaves_like<ListeningToTheLocationService<GeofenceService>> it_has_subscribed_to_change_events = () => { };
    }

    [Subject(typeof(GeofenceService))]
    public class when_the_device_changes_location_and_is_inside_the_geofence : WithConcreteSubject<GeofenceService, IGeofenceService>
    {
        Establish context = () =>
            {
                With<NewGeofenceService>();
                With<LocationListInsideFence>();
            };

        Because of = () => NewGeofenceService.LocationChanged(LocationListInsideFence.NewLocations);

        It should_set_the_current_location_to_the_new_location =
            () => Subject.CurrentLocation.ShouldEqual(LocationListInsideFence.NewLocations[0].Coordinate);

        It should_fire_the_inside_fence_event =
            () => NewGeofenceService.InsideFenceEventFired.ShouldBeTrue();
    }

    [Subject(typeof(GeofenceService))]
    public class when_the_device_changes_location_and_is_inside_the_geofence_and_was_previously_inside : WithConcreteSubject<GeofenceService, IGeofenceService>
    {
        Establish context = () =>
            {
                With<NewGeofenceService>();
                With<LocationListInsideFence>();
            };

        Because of = () =>
            {
                NewGeofenceService.LocationChanged(LocationListInsideFence.NewLocations);
                NewGeofenceService.LocationChanged(LocationListInsideFence.NewLocations);
            };


        It should_not_fire_the_inside_fence_event_again =
            () => NewGeofenceService.InsideFenceEventFiredCount.ShouldEqual(1);
    }

    [Subject(typeof(GeofenceService))]
    public class when_the_device_changes_location_and_is_outside_the_geofence : WithConcreteSubject<GeofenceService, IGeofenceService>
    {
        Establish context = () =>
            {
                With<NewGeofenceService>();
                With<LocationListOutsideFence>();
            };

        Because of = () => NewGeofenceService.LocationChanged(LocationListOutsideFence.Locations);

        It should_fire_the_outside_fence_event =
            () => NewGeofenceService.OutsideFenceEventFired.ShouldBeTrue();

    }

    [Subject(typeof(GeofenceService))]
    public class when_the_device_changes_location_and_is_outside_the_geofence_and_was_previously_outside : WithConcreteSubject<GeofenceService, IGeofenceService>
    {
        Establish context = () =>
            {
                With<NewGeofenceService>();
                With<LocationListOutsideFence>();
            };

        Because of = () =>
            {
                NewGeofenceService.LocationChanged(LocationListOutsideFence.Locations);
                NewGeofenceService.LocationChanged(LocationListOutsideFence.Locations);
            };

        It should_not_fire_the_outside_fence_event_again =
            () => NewGeofenceService.OutsideFenceEventFiredCount.ShouldEqual(1);

    }

    [Subject(typeof(GeofenceService), "initializing")]
    public class when_initialize_is_called : WithConcreteSubjectAndResult<GeofenceService, IGeofenceService, bool>
    {
        Establish context = () =>
            {
                With<NewGeofenceService>();
                With<LocationListInsideFence>();                
            };
        
        Because of = () =>
            {
                var task = Task.Factory.StartNew(() => Result = Subject.Initialize());
                NewGeofenceService.LocationChanged(LocationListInsideFence.NewLocations);

                task.Wait();
            };

        It should_tell_the_location_listener_to_start_listening = () =>
            The<MockLocationListener>().WasToldTo(call => call.StartListening(Param<LocationSettings>.IsAnything));
    }

    [Subject(typeof(GeofenceService), "disposing")]
    public class When_disposing : WithConcreteSubjectAndResult<GeofenceService, IGeofenceService, bool>
    {
        Establish context = () =>
        {
            With<NewGeofenceService>();
            With<LocationListInsideFence>();
        };

        Because of = () => Subject.Dispose();

        It should_tell_the_location_listener_to_stop_listening = () => 
            The<MockLocationListener>().WasToldTo(call=> call.StopListening());
    }

    [Subject(typeof(GeofenceService), "initialize")]
    public class when_initialize_is_called_and_device_recieves_location_update : WithConcreteSubjectAndResult<GeofenceService, IGeofenceService, bool>
    {
        static Task InitializeTask;

        Establish context = () =>
            {
                With<NewGeofenceService>();
                With<LocationListInsideFence>();                
            };
        
        Because of = () =>
            {
                InitializeTask = Task.Factory.StartNew(() => Result = Subject.Initialize());
                NewGeofenceService.LocationChanged(LocationListInsideFence.NewLocations);

                InitializeTask.Wait();
            };

        It should_call_the_distance_calcualtion_service_to_check_if_its_still_inside_the_fence = () =>
            The<IDistanceCalculatorService>().WasToldTo(call => call.DistanceBetween(LocationListInsideFence.NewLocations[0].Coordinate, Param<LocationCoordinate>.IsAnything));

        It should_return_true = () => Result.ShouldEqual(true);
    }

    [Subject(typeof(GeofenceService), "initialize")]
    public class when_initialize_is_called_and_device_does_not_recieve_a_location_update :
        WithConcreteSubjectAndResult<GeofenceService, IGeofenceService, bool>
    {
        static Task InitializeTask;

        Establish context = () =>
        {
            With<NewGeofenceService>();
            With<LocationListInsideFence>();
        };

        Because of = () =>
        {
            InitializeTask = Task.Factory.StartNew(() => Result = Subject.Initialize());
            InitializeTask.Wait();
        };

        It should_return_false = () => Result.ShouldEqual(false);
    }
}