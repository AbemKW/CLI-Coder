# FileIOAgent

A powerful .NET 9 AI-powered file system assistant and retrieval-augmented generation (RAG) chatbot for advanced local file management, semantic search, and natural language interaction.

## Overview

**FileIOAgent** is a C# application that transforms your local file system into an intelligent, searchable, and interactive workspace. It combines modern AI models, semantic search, and a plugin-based architecture to let you manage, analyze, and retrieve information from your files using natural language. The system is designed for extensibility, privacy, and ease of use.

## Key Features

- **Conversational AI Chatbot**: Interact with your file system using natural language queries and commands.
- **Comprehensive File Management**: Read, write, update, delete, and list files and directories securely within a sandboxed workspace.
- **Retrieval-Augmented Generation (RAG)**: Indexes and semantically searches file and web content using vector embeddings (MiniLM) for context-aware retrieval.
- **Web Content Ingestion**: Fetches and chunks website content for inclusion in your semantic search index.
- **Extensible Plugin System**: Easily add new capabilities via plugins (e.g., file analysis, search, terminal command execution).
- **Terminal Command Execution**: Run shell commands safely within your workspace and get the output in chat.
- **Privacy & Security**: All operations are sandboxed to a dedicated workspace directory, with strict path validation and privacy rules.
- **Modern .NET 9 & C# 13**: Built with the latest .NET and C# features for performance and maintainability.

## How It Works

- The application creates a dedicated workspace directory in your user profile (`FileAnalyzerWorkSpace`).
- All file operations, indexing, and search are restricted to this workspace for safety.
- The RAG engine (see `RAG.cs`) chunks and encodes documents and web content using MiniLM embeddings, enabling fast and accurate semantic search.
- Plugins like `FileAnalyzerPlugin` and `FileSearchPlugin` provide file management, search, and analysis capabilities, all accessible via natural language.
- The main chat loop (see `Program.cs`) lets you converse with the AI, which can answer questions, search files, and execute commands.

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Access to an OpenAI-compatible API endpoint (or local LLM server)
- (Optional) [SentenceTransformers.MiniLM](https://www.nuget.org/packages/SentenceTransformers.MiniLM) and [ReadSharp](https://www.nuget.org/packages/ReadSharp) NuGet packages

### Installation

1. Clone this repository.
2. Restore dependencies:dotnet restore3. Build the project:dotnet build
### Usage

1. Run the application:dotnet run --project FileIOAgent2. Interact with the chatbot in your terminal. Example commands:
   - "List all files in the workspace."
   - "Read the first 100 characters of README.md."
   - "Search for the word 'plugin' in all files."
   - "Add the content of https://example.com to the search index."
   - "Delete file notes.txt."
   - "Run the command 'dir' or 'ls'."

## Project Structure

- `Program.cs`: Main entry point; sets up the AI kernel, plugins, and chat loop.
- `RAG.cs`: Retrieval-augmented generation logic for chunking, encoding, and searching documents and web content.
- `FileAnalyzerPlugin.cs`: Provides file reading, writing, updating, deleting, and directory listing functions.
- `FileSearchPlugin.cs`: Adds advanced file search and content discovery features.
- `system.md`: System prompt and operational guidelines for the AI assistant.
- `FileIOAgent.csproj`: Project configuration and dependencies.

## Extending the System

- Add new plugins by implementing classes with `[KernelFunction]` methods.
- Extend the RAG engine to support new data sources or chunking strategies.
- Customize the system prompt in `system.md` to change the AI's behavior.

## Configuration

- The workspace directory is set to your user profile's `FileAnalyzerWorkSpace` folder.
- Update the OpenAI endpoint and model in `Program.cs` as needed.
- All file and command operations are sandboxed for safety.

---

*Empower your local files with AI: search, manage, and analyze—all through natural language.*

