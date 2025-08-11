# Contributing to Beast Mode Social Creator

Thank you for your interest in contributing to the Beast Mode Social Creator! This project demonstrates enterprise-grade .NET Aspire development patterns and we welcome contributions that align with these high standards.

## 🚀 Quick Start for Contributors

1. **Fork and Clone**
   ```bash
   git clone https://github.com/spboyer/aspire-beast-social3.git
   cd aspire-beast-social3
   ```

2. **Prerequisites**
   - .NET 9.0 SDK or later
   - Docker Desktop (for container services)
   - Visual Studio 2022 or VS Code with C# extension

3. **Run the Application**
   ```bash
   cd SocialContentCreator
   dotnet run --project SocialContentCreator.AppHost
   ```

4. **Access the Dashboard**
   - Aspire Dashboard: https://localhost:17180
   - Web Application: https://localhost:7220

## 🛠️ Development Guidelines

### .NET Aspire Best Practices
- **Always** use .NET 9+ for new projects (never .NET 8)
- **Always** use `dotnet run --project AppHost` for orchestration
- **Always** implement health checks with `/health` endpoints
- **Always** use service discovery for inter-service communication
- **Always** follow the AppHost-first architecture pattern

### Code Standards
- Follow C# coding conventions and use nullable reference types
- Use Entity Framework Core with proper migrations
- Implement proper error handling and logging
- Add comprehensive unit tests for new features
- Use Tailwind CSS for UI components (maintain design consistency)

### Architecture Patterns
- Microservices with .NET Aspire orchestration
- Service defaults for shared configurations
- Azure-ready with Container Apps deployment patterns
- Modern UI with Blazor and Tailwind CSS
- AI integration patterns for content generation

## 📋 Contribution Areas

### High Priority
- [ ] Azure OpenAI integration for real AI content generation
- [ ] Social media platform API integrations (Twitter, LinkedIn, Facebook)
- [ ] Analytics dashboard with performance metrics
- [ ] Content calendar and scheduling functionality
- [ ] User authentication with Azure AD B2C

### Medium Priority
- [ ] Team collaboration features
- [ ] Brand voice management
- [ ] Advanced content templates
- [ ] Performance optimizations
- [ ] Additional platform integrations

### Documentation
- [ ] API documentation
- [ ] Deployment guides
- [ ] Architecture decision records
- [ ] Tutorial content

## 🧪 Testing

### Running Tests
```bash
dotnet test
```

### Test Requirements
- Unit tests for all service layers
- Integration tests for API endpoints
- Health check validation tests
- UI component tests

## 📦 Pull Request Process

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Changes**
   - Follow the coding standards
   - Add tests for new functionality
   - Update documentation as needed

3. **Test Locally**
   ```bash
   dotnet test
   dotnet run --project SocialContentCreator.AppHost
   ```

4. **Submit Pull Request**
   - Use descriptive commit messages
   - Reference any related issues
   - Include screenshots for UI changes
   - Ensure all checks pass

### PR Requirements
- [ ] Code builds successfully
- [ ] All tests pass
- [ ] Health checks work correctly
- [ ] Documentation updated
- [ ] UI changes tested on multiple screen sizes

## 🔍 Code Review Criteria

### Technical
- Follows .NET Aspire patterns and best practices
- Proper error handling and logging
- Performance considerations
- Security best practices (Managed Identity, no connection strings)

### Architecture
- Maintains microservices separation
- Uses service discovery correctly
- Follows established patterns
- Azure deployment ready

### UI/UX
- Consistent with Tailwind CSS design system
- Responsive design
- Accessibility considerations
- Professional appearance

## 🎯 Getting Help

- **Issues**: Use GitHub Issues for bug reports and feature requests
- **Discussions**: Use GitHub Discussions for questions and ideas
- **Documentation**: Check the README.md and `/docs` folder
- **Examples**: Look at existing code for patterns and conventions

## 📄 License

By contributing, you agree that your contributions will be licensed under the same license as the project.

---

**Thank you for helping make Beast Mode Social Creator even better! 🚀**
