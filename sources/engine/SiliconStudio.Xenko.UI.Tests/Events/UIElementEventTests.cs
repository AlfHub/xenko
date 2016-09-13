﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using NUnit.Framework;

using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Events;
using SiliconStudio.Xenko.UI.Panels;
using SiliconStudio.Xenko.UI.Tests.Layering;

namespace SiliconStudio.Xenko.UI.Tests.Events
{
    /// <summary>
    /// A class that contains test functions for layering of the UIElement class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for UIElement events")]
    public class UIElementEventTests : UIElement
    {
        protected override IEnumerable<IUIElementChildren> EnumerateChildren()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs all the tests
        /// </summary>
        public void TestAll()
        {
            TestAddRemoveHandler();
            TestRaiseEvent();
            TestPropagateEvent();
        }

        private void TestDelegate2(Object sender, RoutedEventArgs args)
        {
        }

        /// <summary>
        /// Tests for functions <see cref="UIElement.AddHandler{T}"/> and <see cref="UIElement.RemoveHandler{T}"/>
        /// </summary>
        [Test]
        public void TestAddRemoveHandler()
        {
            var testRoutedEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>("Test1", RoutingStrategy.Tunnel, typeof(UIElementLayeringTests));
            var element = new UIElementLayeringTests();

            // test for ArgumentNullException
            Assert.Throws<ArgumentNullException>(() => element.AddHandler<RoutedEventArgs>(null, TestDelegate2));
            Assert.Throws<ArgumentNullException>(() => element.AddHandler(testRoutedEvent, null));
            Assert.Throws<ArgumentNullException>(() => element.RemoveHandler<RoutedEventArgs>(null, TestDelegate2));
            Assert.Throws<ArgumentNullException>(() => element.RemoveHandler(testRoutedEvent, null));

            // test that adding and removing 2 times the same element does not throws any exceptions
            element.AddHandler(testRoutedEvent, TestDelegate2);
            element.AddHandler(testRoutedEvent, TestDelegate2);
            element.RemoveHandler(testRoutedEvent, TestDelegate2);
            element.RemoveHandler(testRoutedEvent, TestDelegate2);
            element.RemoveHandler(testRoutedEvent, TestDelegate2);
        }

        private RoutedEventArgs argsPassedToRaiseEvent = new RoutedEventArgs();

        private static readonly RoutedEvent<RoutedEventArgs> eventPassedToRaiseEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>("TestRaiseEvent", RoutingStrategy.Tunnel, typeof(UIElementLayeringTests));

        private UIElement sourcePassedToRaiseEvent;

        private bool enteredInTestArgsDelegate;

        private void TestArgsDelegate(object sender, RoutedEventArgs e)
        {
            Assert.AreEqual(argsPassedToRaiseEvent, e);
            Assert.AreEqual(eventPassedToRaiseEvent, e.RoutedEvent);
            Assert.AreEqual(sourcePassedToRaiseEvent, e.Source);
            Assert.Throws<InvalidOperationException>(() => e.Source = null);
            Assert.Throws<InvalidOperationException>(() => e.RoutedEvent = null);
            Assert.AreEqual(false, e.Handled);

            enteredInTestArgsDelegate = true;
        }

        /// <summary>
        /// Test for <see cref="UIElement.RaiseEvent"/>
        /// </summary>
        [Test]
        public void TestRaiseEvent()
        {
            // Test ArgumentNullException
            Assert.Throws<ArgumentNullException>(() => RaiseEvent(null));

            AddHandler(eventPassedToRaiseEvent, TestArgsDelegate);

            // test that if RoutedEvent of argument is null nothing special happens
            RaiseEvent(new RoutedEventArgs());

            // test the values of the arguments in the delegate
            sourcePassedToRaiseEvent = this;
            argsPassedToRaiseEvent = new RoutedEventArgs(eventPassedToRaiseEvent);
            RaiseEvent(argsPassedToRaiseEvent);
            sourcePassedToRaiseEvent = new UIElementLayeringTests();
            argsPassedToRaiseEvent = new RoutedEventArgs(eventPassedToRaiseEvent, sourcePassedToRaiseEvent);
            RaiseEvent(argsPassedToRaiseEvent);

            // check that the delegate has been called
            Assert.AreEqual(true, enteredInTestArgsDelegate);

            // check that value of the event raised can be modified again after being raised
            Assert.DoesNotThrow(() => argsPassedToRaiseEvent.RoutedEvent = null);
            Assert.DoesNotThrow(() => argsPassedToRaiseEvent.Source = null);

            // test InvalidOperationException
            var eventMyTest = EventManager.RegisterRoutedEvent<MyTestRoutedEventArgs>("myEventTestRaise", RoutingStrategy.Direct, typeof(UIElementLayeringTests));
            Assert.Throws<InvalidOperationException>(() => RaiseEvent(new RoutedEventArgs(eventMyTest)));
        }

        private readonly List<Object> senderList = new List<object>();

        private void TestAddSenderToList(Object sender, RoutedEventArgs e)
        {
            senderList.Add(sender);
        }

        private bool testMyTestHandlerCalled;

        private void TestMyTestHandler(Object sender, MyTestRoutedEventArgs e)
        {
            testMyTestHandlerCalled = true;
        }
        private bool testEventHandledTooCalled;

        private void TestEventHandledHandler(Object sender, RoutedEventArgs e)
        {
            testEventHandledTooCalled = true;
        }

        private void TestHandledHandler(Object sender, RoutedEventArgs e)
        {
            senderList.Add(sender);
            e.Handled = true;
        }


        private readonly List<Object> classHandlerSenderList = new List<object>();

        private void TestAddSenderToClassHandlerList(Object sender, RoutedEventArgs e)
        {
            classHandlerSenderList.Add(sender);
        }
        private void TestClassHandlerHandled(Object sender, RoutedEventArgs e)
        {
            classHandlerSenderList.Add(sender);
            e.Handled = true;
        }

        private bool testClassHandlerEventHandledTooCalled;

        private void TestClassHandlerEventHandled(Object sender, RoutedEventArgs e)
        {
            testClassHandlerEventHandledTooCalled = true;
        }

        /// <summary>
        /// Test for <see cref="UIElement.PropagateRoutedEvent"/>
        /// </summary>
        [Test]
        public void TestPropagateEvent()
        {
            // create a hierarchy of UIElements
            //               (00)
            //                | 
            //               (10)
            //             /     \
            //          (20)      (21)
            //            |       /  \
            //          (30)    (31)  (32)
            var element00 = new ContentControlTest();
            var element10 = new StackPanel();
            var element20 = new ContentControlTest();
            var element21 = new StackPanel();
            var element30 = new UIElementLayeringTests();
            var element31 = new UIElementLayeringTests();
            var element32 = new UIElementLayeringTests();
            element00.Content = element10;
            element10.Children.Add(element20);
            element10.Children.Add(element21);
            element20.Content = element30;
            element21.Children.Add(element31);
            element21.Children.Add(element32);
            var elements = new List<UIElement> { element00, element10, element20, element21, element30, element31, element32 };

            // create routed events
            var tunnelingEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>("TestTuneling", RoutingStrategy.Tunnel, typeof(UIElementLayeringTests));
            var bubblingEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>("TestBubbling", RoutingStrategy.Bubble, typeof(UIElementLayeringTests));
            var directEvent = EventManager.RegisterRoutedEvent<RoutedEventArgs>("TestDirect", RoutingStrategy.Direct, typeof(UIElementLayeringTests));

            // test propagation direction, propagation bounds and sender values
            foreach (var uiElement in elements)
            {
                uiElement.AddHandler(tunnelingEvent, TestAddSenderToList);
                uiElement.AddHandler(bubblingEvent, TestAddSenderToList);
                uiElement.AddHandler(directEvent, TestAddSenderToList);
            }

            // tunneling test 1
            senderList.Clear();
            element20.RaiseEvent(new RoutedEventArgs(tunnelingEvent));
            Assert.AreEqual(3, senderList.Count);
            Assert.AreEqual(element00, senderList[0]);
            Assert.AreEqual(element10, senderList[1]);
            Assert.AreEqual(element20, senderList[2]);

            // tunneling test 2
            senderList.Clear();
            element31.RaiseEvent(new RoutedEventArgs(tunnelingEvent));
            Assert.AreEqual(4, senderList.Count);
            Assert.AreEqual(element00, senderList[0]);
            Assert.AreEqual(element10, senderList[1]);
            Assert.AreEqual(element21, senderList[2]);
            Assert.AreEqual(element31, senderList[3]);

            // direct test
            senderList.Clear();
            element10.RaiseEvent(new RoutedEventArgs(directEvent));
            Assert.AreEqual(1, senderList.Count);
            Assert.AreEqual(element10, senderList[0]);

            // tunneling test 1
            senderList.Clear();
            element30.RaiseEvent(new RoutedEventArgs(bubblingEvent));
            Assert.AreEqual(4, senderList.Count);
            Assert.AreEqual(element30, senderList[0]);
            Assert.AreEqual(element20, senderList[1]);
            Assert.AreEqual(element10, senderList[2]);
            Assert.AreEqual(element00, senderList[3]);

            // tunneling test 2
            senderList.Clear();
            element20.RaiseEvent(new RoutedEventArgs(bubblingEvent));
            Assert.AreEqual(3, senderList.Count);
            Assert.AreEqual(element20, senderList[0]);
            Assert.AreEqual(element10, senderList[1]);
            Assert.AreEqual(element00, senderList[2]);

            // test with another type of handler
            var eventMyTestHandler = EventManager.RegisterRoutedEvent<MyTestRoutedEventArgs>("TestMyTestHandler", RoutingStrategy.Direct, typeof(UIElementLayeringTests));
            AddHandler(eventMyTestHandler, TestMyTestHandler);
            RaiseEvent(new MyTestRoutedEventArgs(eventMyTestHandler));
            Assert.AreEqual(true, testMyTestHandlerCalled);

            // test Handled and EventHandledToo
            foreach (var uiElement in elements)
            {
                uiElement.RemoveHandler(bubblingEvent, TestAddSenderToList);
                uiElement.AddHandler(bubblingEvent, TestHandledHandler);
            }
            senderList.Clear();
            element00.AddHandler(bubblingEvent, TestEventHandledHandler, true);
            element32.RaiseEvent(new RoutedEventArgs(bubblingEvent));
            Assert.AreEqual(1, senderList.Count);
            Assert.AreEqual(element32, senderList[0]);
            Assert.AreEqual(true, testEventHandledTooCalled);

            // test class handlers basic working
            foreach (var uiElement in elements)
                uiElement.RemoveHandler(bubblingEvent, TestHandledHandler);
            EventManager.RegisterClassHandler(typeof(ContentControl), bubblingEvent, TestAddSenderToClassHandlerList);
            element30.RaiseEvent(new RoutedEventArgs(bubblingEvent));
            Assert.AreEqual(2, classHandlerSenderList.Count);
            Assert.AreEqual(element20, classHandlerSenderList[0]);
            Assert.AreEqual(element00, classHandlerSenderList[1]);

            // test that class handlers are called before instance handlers + test handledEventToo for class handlers
            senderList.Clear();
            classHandlerSenderList.Clear();
            EventManager.RegisterClassHandler(typeof(ContentControl), bubblingEvent, TestClassHandlerHandled);
            EventManager.RegisterClassHandler(typeof(StackPanel), bubblingEvent, TestClassHandlerEventHandled, true);
            foreach (var uiElement in elements)
                uiElement.AddHandler(bubblingEvent, TestAddSenderToList);
            element20.RaiseEvent(new RoutedEventArgs(bubblingEvent));
            Assert.AreEqual(1, classHandlerSenderList.Count);
            Assert.AreEqual(element20, classHandlerSenderList[0]);
            Assert.AreEqual(0, senderList.Count);
            Assert.AreEqual(true, testClassHandlerEventHandledTooCalled);
        }

        /// <summary>
        /// Test that the handlers can be detached inside the handler itself
        /// </summary>
        [Test]
        public void TestUnregisterHandlerInsideHandler()
        {
            testUnregisterHandlerCallCount = 0;

            var button = new Button();
            button.Click += TestUnregisterHandlerOnClick;
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.AreEqual(1, testUnregisterHandlerCallCount);
        }

        private int testUnregisterHandlerCallCount;

        private void TestUnregisterHandlerOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ++testUnregisterHandlerCallCount;

            ((Button)sender).Click -= TestUnregisterHandlerOnClick;

            if(testUnregisterHandlerCallCount < 10) // avoid infinite looping on test fail
                ((Button)sender).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        /// <summary>
        /// Test that the handlers are raised in the same order as they are added.
        /// </summary>
        [Test]
        public void TestHandlerRaiseOrder()
        {
            lastHandlerCalledId = 0;

            var button = new Button();
            button.Click += TestHandlerRaiseOrderOnClick1;
            button.Click += TestHandlerRaiseOrderOnClick2;
            button.Click += TestHandlerRaiseOrderOnClick3;
            button.Click += TestHandlerRaiseOrderOnClick4;

            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.AreEqual(4, lastHandlerCalledId);
        }

        private int lastHandlerCalledId;

        private void TestHandlerRaiseOrderOnClick1(object sender, RoutedEventArgs routedEventArgs)
        {
            Assert.AreEqual(0, lastHandlerCalledId);
            lastHandlerCalledId = 1;
        }
        private void TestHandlerRaiseOrderOnClick2(object sender, RoutedEventArgs routedEventArgs)
        {
            Assert.AreEqual(1, lastHandlerCalledId);
            lastHandlerCalledId = 2;
        }
        private void TestHandlerRaiseOrderOnClick3(object sender, RoutedEventArgs routedEventArgs)
        {
            Assert.AreEqual(2, lastHandlerCalledId);
            lastHandlerCalledId = 3;
        }
        private void TestHandlerRaiseOrderOnClick4(object sender, RoutedEventArgs routedEventArgs)
        {
            Assert.AreEqual(3, lastHandlerCalledId);
            lastHandlerCalledId = 4;
        }

        /// <summary>
        /// Test for recursive <see cref="UIElement.RaiseEvent"/>
        /// </summary>
        [Test]
        public void TestReccursiveRaise()
        {
            clickCount = 0;

            var button = new Button();
            button.Click += TestReccursiveRaiseOnClick;
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.AreEqual(10, clickCount);
        }

        private int clickCount;

        private void TestReccursiveRaiseOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ++clickCount;

            if (clickCount < 10)
                ((Button)sender).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }
}
