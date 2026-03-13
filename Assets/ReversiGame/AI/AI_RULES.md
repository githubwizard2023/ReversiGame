AI Coding Standard

Mandatory Rules

Naming
- Constants must use UPPER_SNAKE_CASE.
- Variables, methods, classes, fields, properties, and files must use meaningful names.
- Avoid vague names such as data, temp, obj, thing, manager, or handler unless the responsibility is truly broad.
- Boolean names must read like a state, condition, or question.
- Each field should be as meaningful and intention-revealing as possible.

Code Structure
- Keep classes focused on a single responsibility.
- Keep methods focused on a single purpose.
- Keep gameplay logic separated from presentation logic whenever possible.
- Prefer small, composable units over large multi-purpose classes.
- Favor composition over unnecessary inheritance.
- Prefer enums over booleans when a value represents a meaningful state.
- Example: prefer `DoorState { Locked, Unlocked }` over a boolean such as `isUnlocked`.
- Avoid boolean method parameters when an enum or dedicated parameter object would make intent clearer.

Architecture
- Use Zenject and dependency injection when dependencies should be supplied from outside the class.
- Avoid hard-coded dependencies when injection creates a cleaner boundary.
- Use signals for decoupled communication when multiple systems need to react to the same event.
- Do not use signals when a direct dependency is simpler, clearer, and sufficient.
- Prefer decoupled solutions over tightly coupled solutions.
- Use interfaces only when they create a real boundary, improve flexibility, or support testing.

Design Patterns
- Use design patterns only when they solve a real problem in the solution.
- Do not force a pattern into simple code that does not need one.
- If a pattern is used, the class-level comment must explain why that pattern is appropriate.
- Prefer patterns that reduce coupling, clarify responsibilities, and improve maintainability.

Comments
- Do not use <summary> tags.
- Add exactly 3 lines above each class.
- The class comment must explain why the class exists, what pattern or architectural role it follows, and why that choice fits the solution.
- Add 1-2 lines above each method when the explanation adds useful intent or behavioral context.
- Do not add comments that only restate obvious code.

Unity
- Keep MonoBehaviours thin whenever possible.
- Move reusable logic into plain C# classes when MonoBehaviour is not required.
- Serialize only what should be configured in the Inspector.
- Do not expose fields publicly unless there is a clear reason.
- Use DOTween for animations and tween-based transitions.
- Prefer DOTween over custom timing code when tweening is the correct tool.

Control Flow And Formatting
- Do not use while (true) loops.
- Always use braces {} for if, else, for, foreach, while, and similar control statements, even for a single statement.
- Do not use single-line control statements.
- Write control flow in multiline form.
- Keep opening and closing braces explicit and consistent.
- Prefer readable line breaks over compressed one-line logic.
- Do not hide important logic inside chained expressions when a few explicit lines are clearer.

Code Quality
- Prefer simple, readable solutions over clever ones.
- Keep methods short and readable.
- Use early returns when they make the flow easier to follow.
- Avoid deeply nested logic when a flatter structure is clearer.
- Do not use hard-coded values; use named constants or variables instead.
- Do not add unnecessary abstractions.
- Match existing project patterns unless there is a clear and justified improvement.
- Avoid methods that mix setup, business logic, and presentation behavior.

Preferred Guidelines

Problem Solving
- Prefer solutions that are modular, easy to extend, and easy to test.
- Reuse existing systems before introducing new layers or patterns.
- Choose the most maintainable solution, not the most generic one.

Dependency Usage
- Prefer constructor injection where practical and supported by the project setup.
- Keep dependency boundaries explicit.
- Do not introduce interfaces, wrappers, or services without a clear benefit.
- Zenject does not require every class to have an interface.
- Create an interface only when it defines a real boundary, supports multiple implementations, improves testing, or protects higher-level code from concrete details.
- It is valid to bind concrete classes directly in Zenject when no real abstraction is needed.

Working With AI
- Do not generate placeholder architecture without a real use case.
- Do not create generic wrappers or utility layers unless they solve a repeated problem.
- Follow project conventions first, then general best practices.
- If a naming or architecture decision is unclear, choose the more readable and maintainable option.

Anti-Overengineering
- Do not create an interface for every class by default.
- Do not split one cohesive responsibility across many tiny classes unless that separation solves a real problem.
- Prefer one clear concrete class over a chain of interface, wrapper, service, handler, and factory types with no strong reason.
- Add a new class only when it owns a distinct responsibility, lifecycle, or dependency set.
- Add a new interface only when it creates a meaningful architectural boundary.
- Prefer direct injection of a concrete type when the codebase does not benefit from an abstraction.
- Keep the number of moving parts low unless complexity requires otherwise.

Comment Intent
- Class comments should explain the architectural role of the class, not just its name.
- Method comments should explain intent, side effects, or important behavior when useful.
