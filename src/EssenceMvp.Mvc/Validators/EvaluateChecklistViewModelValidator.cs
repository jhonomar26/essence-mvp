using EssenceMvp.Mvc.Models;
using FluentValidation;

namespace EssenceMvp.Mvc.Validators;

public class EvaluateChecklistViewModelValidator : AbstractValidator<EvaluateChecklistViewModel>
{
    public EvaluateChecklistViewModelValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.AlphaId).GreaterThan(0);
        RuleFor(x => x.Answers).NotEmpty();
        RuleForEach(x => x.Answers).SetValidator(new ChecklistAnswerValidator());
    }

    private class ChecklistAnswerValidator : AbstractValidator<EvaluateChecklistViewModel.ChecklistAnswer>
    {
        public ChecklistAnswerValidator()
        {
            RuleFor(x => x.StateChecklistId).GreaterThan(0);
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}
