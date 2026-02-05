using FunctionalStateMachine.CommandRunner;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.CommandRunners;

/// <summary>
/// Handles PlaySoundCommand by playing different sound effects to the user.
/// In a real system, this would interface with the machine's audio system.
/// Here we just print text representations of the sounds.
/// </summary>
public class PlaySoundHandler : ICommandRunner<PlaySoundCommand>
{
    public void Run(PlaySoundCommand command)
    {
        var sound = command.Sound switch
        {
            VendingSound.SelectionConfirmed => "🔊 *beep*",
            VendingSound.TransactionComplete => "🔊 *ding!*",
            VendingSound.ErrorSound => "🔊 *buzz - error sound*",
            VendingSound.DispensingSound => "🔊 *whirrrr*",
            VendingSound.CoinReturn => "🔊 *coin return chime*",
            VendingSound.JamAlert => "🔊 *ALERT - machine jam detected*",
            _ => "🔊 *sound*"
        };

        Console.WriteLine(sound);
    }
}
