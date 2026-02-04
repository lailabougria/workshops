# Mastering complex event-driven architectures

Check the [agenda](agenda.md) to see where the next workshop will run!

## Abstract

Event-driven architecture promises to significantly improve our systems in 
terms of scalability, resilience, and maintainability. It also enables the 
design of loosely coupled components, improving the autonomy and evolution of 
individual services. And sending messages or publishing events is pretty 
straightforward, so what's not to like? Well, it turns out that making all 
those messages and events work together in meaningful ways is the real 
challenge.

Our systems are filled with business processes that are anything but trivial.
They exceed the boundaries of a single method- or service invocation and 
require us to redefine many concepts, from how we think about the domain 
entities and how coupling affects our designs to how we handle time-based 
requirements and the joys of eventual consistency. The true power of 
event-driven architectures surfaces when we start expressing complex 
business processes with intention-revealing messages (both commands and
events) to reflect what's happening in the real world.

If you've moved beyond the basic building blocks of messaging and want to 
transition your designs from meaningless and often tightly coupled 
service-to-service calls to carefully designed processes that reflect your 
business domain, this workshop is for you.

## Who should attend?

This workshop is ideal for those who:

- Have a foundational understanding of messaging as an architectural style 
  and the benefits it offers.
- Understand the basic message-based communication patterns, including 
  request-reply and publish-subscribe.
- Understand concepts like idempotency, eventual consistency and Outbox.
- Have experience using message brokers such as Azure Service Bus, RabbitMQ, 
  Amazon SQS or other queuing or broker technologies.

## What will you learn?

Throughout this workshop, you’ll learn:

- How to identify complex business processes within your domain
- Techniques to help identify the right service boundaries
- Coordinate complex business processes with orchestration and choreography
- Engage in thorough tradeoff analysis to get to the right design
- Manage requirements that need to occur at different points in time
- Seamlessly handle out-of-order messages
- Test complex, long-running processes in an automated way
- Leverage observability to troubleshoot issues where they matter most: in production!
- How to deal with the inevitable: change...

Throughout this workshop, we'll divide into smaller groups for design 
exercises that require nothing more than your imagination and something to 
write with. Later, we'll switch gears and get more hands-on with some coding 
exercises to bring those ideas to life using C#.

Join me and unlock the secrets of messaging's real-world superpowers!

## What should you bring?

This is a Bring Your Own Device (BYOD) workshop. Therefore, attendees are 
required to bring their own device with the necessary software already installed:

- .NET 10 or higher
- Visual Studio/Rider/VS Code
- Docker

In addition, it's helpful to have accounts ready to use for:

- Miro account, for design exercises (a free account is sufficient)
- GitHub, for access to exercises and additional material
