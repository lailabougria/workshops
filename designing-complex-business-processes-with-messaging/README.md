# Designing complex business processes with messaging

Check the [agenda](agenda.md) to see where the next workshop will run!

## Abstract

Messaging in distributed systems offers a range of valuable benefits: it enables the design of decoupled components, where each component can evolve and scale independently. In addition to improving the system’s fault tolerance, it opens the door to building truly self-healing systems. But there's more to explore; the true power of messaging surfaces when we start expressing complex business processes with intention-revealing messages (both commands and events) to reflect what's happening in the real world. These business processes are usually non-trivial, exceed the boundaries of a single method- or service invocation, and require us to redefine certain concepts, including time.

If you've been exposed to the basic building blocks of messaging and want to transition your system from a collection of meaningless and often highly-coupled service-to-service calls to one composed of carefully designed processes that reflect your business use cases, this workshop is for you. We'll discuss numerous real-world scenarios based on years of experience in the banking and retail domains. By discussing various domains, you'll be better equipped to recognise patterns in whichever domain you may be working on.

## Who should attend?

To get the most out of this workshop, it's expected that:
- You have a foundational understanding of messaging as an architectural style and the benefits it offers.
- You understand the basic message-based communication patterns, including request-reply and publish-subscribe.
- You have experience using message brokers, such as Azure Service Bus, RabbitMQ, or other broker technologies.

## What will you learn?

Throughout this workshop, you’ll learn:

- How to identify complex business processes in your domain
- Effective techniques to help design these processes, including:
  - What orchestration and choreography are, and the challenges to consider for each pattern
  - How to deal with actions that occur at different points in time
  - How to deal with out-of-order events seamlessly
  - Performing thorough tradeoff analysis to get to the right design
- How to test complex, long-running processes
- How to leverage observability to troubleshoot issues where it matters most: in production!

Throughout this workshop, we'll dive into design exercises in small groups that need just your imagination and something to write with. Later, we'll switch gears and get our hands on some coding exercises to bring those ideas to life using C#.

Join me and unlock the secrets of messaging's real-world superpowers!

## What should you bring?

This is a Bring Your Own Device (BYOD) workshop. Therefore, attendees are required to bring their own device with the necessary software already installed.

Required software:
- .NET 8 or higher
- Visual Studio/Rider/VS Code
