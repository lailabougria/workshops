# Designing complex business processes with messaging

## Abstract

Messaging in distributed systems offers a range of valuable benefits: it enables the design of decoupled components, where each component can evolve and scale independently. In addition to improving the system’s fault tolerance, it opens the door to building truly self-healing systems. But there's more to explore; the true power of messaging surfaces when we start expressing complex business processes with intention-revealing messages (both commands and events) to reflect what's happening in the real world. These business processes are usually non-trivial, exceed the boundaries of a single method- or service invocation, and require us to redefine certain concepts, including time.

If you've been exposed to the basic building blocks of messaging and want to transition your system from a collection of meaningless and often highly-coupled service-to-service calls to one composed of carefully designed processes that reflect your business use cases, this workshop is for you.

## Who should attend?

To get the most out of this workshop, it's expected that:
- You have a foundational understanding of messaging as an architectural style and the benefits it offers.
- You understand the basic message-based communication patterns, including request-reply and publish-subscribe.
- You have some experience using message brokers, such as Azure Service Bus, RabbitMQ, or other broker technologies.

## What will you learn?

Throughout this workshop, you’ll learn:

- How to identify complex business processes in your domain
- Effective techniques to help design these processes
- What orchestration and choreography are, but more importantly, when and why to use each pattern
- How to deal with actions that occur at different points in time
- How to deal with out-of-order events seamlessly
- How to test complex, long-running processes
- How to leverage observability to troubleshoot issues where it matters most: in production!

Throughout this workshop, we'll dive into design exercises in small groups that need just your imagination and something to write with. Later, we'll switch gears and get our hands on some coding exercises to bring those ideas to life using C#.

Join me and unlock the secrets of messaging's real-world superpowers!
