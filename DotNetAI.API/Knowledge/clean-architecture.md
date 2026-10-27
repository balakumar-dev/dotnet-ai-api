# Clean Architecture

The sample application follows Clean Architecture.

Layers:
- API: HTTP controllers, middleware, and dependency injection registration.
- Core: DTOs, interfaces, and application contracts. Core does not reference OpenAI, Gemini, or any database.
- Infrastructure: AI providers, embeddings, and later database or vector store implementations.

Dependency rule: API depends on Core. Infrastructure depends on Core. Core depends on nothing external.

This is why ChatController injects IAIService instead of GeminiService.
