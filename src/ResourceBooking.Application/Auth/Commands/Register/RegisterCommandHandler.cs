using MediatR;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public RegisterCommandHandler(
        IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Email, passwordHash);

        // Throws DuplicateEmailException if the email is already registered.
        await _userRepository.AddAsync(user, cancellationToken);

        var token = _tokenGenerator.GenerateToken(user);
        return new AuthResultDto(token, user.Id, user.Email, user.Role);
    }
}
