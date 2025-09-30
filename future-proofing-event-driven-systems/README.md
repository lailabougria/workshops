# Future-proofing event-driven systems

Check the [agenda](agenda.md) to see where the next workshop will run!

## Abstract

Event-driven architecture promises to significantly improve our systems in terms of scalability, resilience, and maintainability. It also enables the design of loosely coupled components, improving the autonomy and evolution of individual services. And sending messages or publishing events is pretty straightforward, so what's not to like? Well, it turns out that making all those messages and events work together in meaningful ways is the real challenge, especially considering the continuous changes our system undergoes. We pay a massive amount of effort considering how to design our services, considering where exactly the boundaries of each individual service lie, but what we often see is that as our systems change, interconnections surface where we didn't expect them, contracts change, receivers break, and messages end up in a dead-letter queue nobody looks at. However, if you're in for the long run, what we want to prepare our systems for is to gracefully undergo change, without undoing all the hard work previously done and turning it into a big ball of mud.

If you've moved beyond the basic building blocks of messaging and want to transition your designs from meaningless and often tightly coupled service-to-service calls to carefully designed services that are well prepared to endure change, this workshop is for you.

## Who should attend?

This workshop is ideal for those who:

- Have a foundational understanding of messaging as an architectural style and the benefits it offers.
- Understand the basic message-based communication patterns, including request-reply and publish-subscribe.
- Understand concepts like idempotency, eventual consistency and Outbox.
- Have experience using message brokers such as Azure Service Bus, RabbitMQ, Amazon SQS or other queuing or broker technologies.

Due to the limited time in this workshop, we won't spend any time filling those gaps.

## What will you learn?

Throughout this workshop, you’ll learn:

- Coupling and how it affects the system we build
- Techniques to help identify the right service boundaries that endure change
- Design techniques to help you protect those boundaries (public vs private)
- Serialization, schemas, and why they matter
- Using CloudEvents to standardize message envelopes
- Using xRegistry to manage your endpoints, messages and schemas

Throughout this workshop, we'll divide into smaller groups for design exercises that require nothing more than your imagination and something to write with. Later, we'll switch gears and get more hands-on with some coding exercises to bring those ideas to life using C#.

Join me and unlock the secrets of messaging's real-world superpowers!

## What should you bring?

This is a Bring Your Own Device (BYOD) workshop. Therefore, attendees are required to bring their own device with the necessary software already installed:

- .NET 8 or higher
- Visual Studio/Rider/VS Code
- Docker
- Pen and paper
