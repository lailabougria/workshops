# Orchestration versus Choreography

## Books

The following books cover the topics orchestration and choreography. They're listed in order of relevance:

- [Software Architecture: The hard parts ](https://www.oreilly.com/library/view/software-architecture-the/9781492086888/), by Neal Ford, Mark Richards, Pramod Sadalage, Zhamak Dehghani
- [Practical Process Automation](https://www.oreilly.com/library/view/practical-process-automation/9781492061441/), by Bernd Ruecker
- [Building Microservices: Designing Fine-Grained Systems](https://www.oreilly.com/library/view/building-microservices-designing/9781663728203/), by Sam Newman, Theodore O'Brien

## The 5-question decision-framework

I developed this framework over the years based on my learnings from the field and the [books listed in the resources](#books).
This framework consists of five pillar questions, but it's important to dig further into the details of each pillar.

The five main questions to keep in mind are:
1. What type of communication is suitable? Synchronous or asynchronous communication?
    - Are there business requirements for synchronous communication?
    - Is it acceptable for errors to occur and, in the worst case, for requests to be lost? Is the contrary true?
    - Are there scaling requirements that may affect this decision?
2. Which direction of coupling is preferred? Does it make sense to couple the sender to the receiver or the receiver to a publisher?
    - Does it make sense for the sender to be coupled to the receiver and introduce command coupling?
    - Is it more suitable to decouple the sender from the receiver and instead rely on the inverse coupling?
3. Are there complex compensating flows? Would they introduce significant bidirectional coupling?
    - How many compensation flows are there in this business process?
    - How extensive are they? Which services do they impact?
    - How many "hops back" are required to complete a compensating flow? How much bidirectional coupling does this introduce?
4. Is there a high probability of change?
    - Are you working on a relatively stable domain? Or is change on the horizon?
    - What are some of the most plausible changes that may need to be implemented here?
    - How would these affect your current design?
5. Is there someone responsible for the end-to-end flow?
    - If a workflow is stuck, who would be responsible for it?
    - Who would be responsible/accountable for the flow?
    - Are there parts of the workflow with high responsibility involved that could be isolated?

Finally, it's important to continuously reassess the scope of the workflow and question whether some parts can be isolated into dedicated workflows.
Remember to draw your workflow using both styles. A visual representation can be useful for surfacing hidden requirements and helping visualize the impact of change. It's also a helpful tool in discussions with business experts.

## Talks

See [my talk](https://github.com/lailabougria/talks/tree/main/orchestration-vs-choreography) on the subject.

## Tooling

### [Azure Logic Apps](https://learn.microsoft.com/en-us/azure/logic-apps/logic-apps-overview)

- Azure Logic Apps is a cloud platform where you can create and run automated workflows with little to no code.
- Logic Apps are a designer-first serverless workflow integration platform.
- Select from prebuilt operations to quickly build a workflow that integrates and manages your apps, data, services, and systems.
- Simplify how you connect legacy, modern, and cutting-edge systems across cloud, on premises, and hybrid environments.
- Provides low-code-no-code toolsto develop highly scalable integration solutions for your enterprise and business-to-business (B2B) scenarios.
- Choose from hundreds of prebuilt connectors so you can connect and integrate apps, data, services, and systems more easily and quickly. - Focus on designing and implementing your solution's business logic and functionality, not on figuring out how to access your resources.

### [Azure durable functions](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-orchestrations?tabs=csharp-inproc)

Durable Functions is an extension of Azure Functions. You can use an orchestrator function to orchestrate the execution of other Durable functions within a function app. Orchestrator functions have the following characteristics:

- Orchestrator functions define function workflows using procedural code. No declarative schemas or designers are needed.
- Orchestrator functions can call other durable functions synchronously and asynchronously. Output from called functions can be reliably saved to local variables.
- Orchestrator functions are durable and reliable. Execution progress is automatically checkpointed when the function "awaits" or "yields". Local state is never lost when the process recycles or the VM reboots.
- Orchestrator functions can be long-running. The total lifespan of an orchestration instance can be seconds, days, months, or never-ending.

[Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-orchestrations)

### [AWS Step Functions](https://aws.amazon.com/step-functions/)

AWS Step Functions is a visual workflow service that helps developers use AWS services to build distributed applications, automate processes, orchestrate microservices, and create data and machine learning (ML)

Use cases include:

- **Orchestrate microservices**: Combine multiple AWS Lambda functions into responsive serverless applications and microservices.
- **Orchestrate large-scale parallel workloads**: Iterate over and process large data-sets such as security logs, transaction data, or image and video files.
- Automate extract, transform, and load (ETL) processes: Ensure that multiple long-running ETL jobs run in order and complete successfully, without the need for manual orchestration.
- Automate security and IT functions: Create automated workflows, including manual approval steps, for security incident response.

### [NServiceBus Sagas](https://docs.particular.net/nservicebus/sagas/)

NServiceBus sagas provide a mechanism to orchestrate long-running business processes. By configuring a persistence mechanism, sagas are also [stateful](https://docs.particular.net/nservicebus/sagas/#long-running-means-stateful), allowing them to store any required workflow state. It's possible to use sagas with both [command-driven](https://docs.particular.net/nservicebus/sagas/) and event-driven communication, but either way, it's in essence a form of orchestration, since it acts as a central point in managing a specific workflow.

NServiceBus sagas can be deployed inside an existing service (on in NServiceBus terminology, an endpoint), or as a dedicated service. It's important to note that [when a saga is completed](https://docs.particular.net/nservicebus/sagas/#ending-a-saga), the saga state is removed from the configured data store.

### [Camunda](https://camunda.com/solutions/microservices-orchestration/)

- **State Handling**: Persists the state of each instance of a business process (e.g., each order placed on an ecommerce website)
- **Explicit Processes**: Makes business processes explicit instead of burying them in code, making it easier for teams to understand and modify them
- **Message Correlation and Coordination**: Merges messages belonging to a single process instance and decides next steps — BPMN automatically implements message patterns such as sequences, synchronization, mutual exclusion, and timeouts
- **Compensation for Problems**: Compensates if a business transaction or process encounters a problem that requires previously completed steps to be undone
- **Timeout Handling**: Tracks the passage of time and automatically takes action or switches to another path in the process flow if an event does not take place as expected
- **Error Handling**: Allows you to specify the behavior that should happen when an error occurs (e.g., retrying an action, taking another path)
- **Transparency of Status**: Enables operations teams to monitor the status of process instances in real time
- **Collaboration**: Provides graphical models of business processes that facilitate discussion between business stakeholders, developers, and operations teams

### [Netflix Conductor](https://conductor.netflix.com/index.html)

[Netflix Conductor](https://netflixtechblog.com/netflix-conductor-a-microservices-orchestrator-2e8d4771bf40) is an orchestration engine aimed to take out the need for boilerplate code in applications, provide a reactive flow and address the following requirements:
- Tracking and management of workflows.
- Ability to pause, resume and restart processes.
- User interface to visualize process flows.
- Ability to synchronously process all the tasks when needed.
- Ability to scale to millions of concurrently running process flows.
- Backed by a queuing service abstracted from the clients.
- Be able to operate over HTTP or other transports e.g. gRPC.

### [Dapr Workflow](https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-overview/)

Dapr workflow provides a stateful solution to support long-running and fault-tolerant applications, ideal for orchestrating microservices. Dapr workflow works seamlessly with other Dapr building blocks, such as service invocation, pub/sub, state management, and bindings.

The durable, resilient Dapr Workflow capability:

- Offers a built-in workflow runtime for driving Dapr Workflow execution.
- Provides SDKs for authoring workflows in code, using any language.
- Provides HTTP and gRPC APIs for managing workflows (start, query, pause/resume, raise event, terminate, purge).
- Integrates with any other workflow runtime via workflow components.

## Additional reading

- [Strangler fig pattern](https://martinfowler.com/bliki/StranglerFigApplication.html), by Martin Fowler
- [Passive-aggressive events](https://martinfowler.com/articles/201701-event-driven.html), by Martin Fowler
