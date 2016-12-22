using System;
using System.Collections.Generic;
using Stateless;

namespace PurchaseProcessExample
{
    internal interface IBus
    {
        void Register(Resource resource);
        void SendMessage(OrderMessage message, OrderEvent orderEvent);
    }

    class Bus : IBus, IOrderChannel
    {
        readonly Dictionary<string, StateMachine<OrderState, OrderEvent>> _machines = new Dictionary<string, StateMachine<OrderState, OrderEvent>>();
        readonly Dictionary<string, OrderMessage> _orders = new Dictionary<string, OrderMessage>();
        readonly Dictionary<OrderResourceType, Resource> _resources = new Dictionary<OrderResourceType, Resource>(); 

        public void Register(Resource resource)
        {
            if (_resources.ContainsKey(resource.ResourceType)) return;
            _resources.Add(resource.ResourceType, resource);
        }

        public void SendMessage(OrderMessage message, OrderEvent orderEvent)
        {
            if (orderEvent == OrderEvent.Access)
            {
                _machines.Add(message.OrderId, MachineFactory.CreateInsance(this, message.OrderId));
                _orders.Add(message.OrderId, message);
            }
            else _orders[message.OrderId] = message;

            if (_machines[message.OrderId].CanFire(orderEvent))
            {
                _machines[message.OrderId].Fire(orderEvent);
            }
            else throw new Exception("Trigger " + orderEvent + " is not valid in " + _machines[message.OrderId].State + " order state");
        }

        public void Dispatch(StateMachine<OrderState, OrderEvent>.Transition transition, string orderId)
        {
            if (transition.Destination == OrderState.NoOrder)
            {
                _machines.Remove(orderId);
                _orders.Remove(orderId);
            }
            else if (transition.Destination == OrderState.Empty)
            {
                if (!_resources.ContainsKey(OrderResourceType.Shop)) throw new Exception("does not contain Shop");
                    _resources[OrderResourceType.Shop].ReceiveMessage(_orders[orderId]);
            }
            else if (transition.Destination == OrderState.Filled)
            {
                if (!_resources.ContainsKey(OrderResourceType.Seller)) throw new Exception("does not contain Seller");
                _resources[OrderResourceType.Seller].ReceiveMessage(_orders[orderId]);
            }
            else if (transition.Destination == OrderState.Paid)
            {
                if (!_resources.ContainsKey(OrderResourceType.Sender)) throw new Exception("does not contain Sender");
                _resources[OrderResourceType.Sender].ReceiveMessage(_orders[orderId]);
            }
            else throw new NotImplementedException("Dispatch for " + transition.Destination + " not implemented!");
        }
    }

    static class MachineFactory
    {
        public static StateMachine<OrderState, OrderEvent> CreateInsance(IOrderChannel channel, string id)
        {
            StateMachine<OrderState, OrderEvent> machine = new StateMachine<OrderState, OrderEvent>(OrderState.None);

            machine.Configure(OrderState.None)
                .Permit(OrderEvent.Access, OrderState.Empty)
                .OnEntry(x => channel.Dispatch(x, id));

            machine.Configure(OrderState.Empty)
                .Permit(OrderEvent.Order, OrderState.Filled)
                .Permit(OrderEvent.Exit, OrderState.NoOrder)
                .OnEntry(x => channel.Dispatch(x, id));

            machine.Configure(OrderState.Filled)
                .Permit(OrderEvent.Pay, OrderState.Paid)
                .Permit(OrderEvent.Modify, OrderState.Empty)
                .OnEntry(x => channel.Dispatch(x, id));

            machine.Configure(OrderState.Paid)
                .Permit(OrderEvent.Exit, OrderState.NoOrder)
                .OnEntry(x => channel.Dispatch(x, id));

            return machine;
        }
    }

    internal interface IOrderChannel
    {
        void Dispatch(StateMachine<OrderState, OrderEvent>.Transition transition, string orderId);
    }
}
