// https://angular.io/guide/form-validation#adding-to-template-driven-forms-1

import { Directive } from '@angular/core';
import {
  NG_VALIDATORS,
  Validator,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
  FormGroup
} from '@angular/forms';
import { nameof } from 'src/app/utils/utils';

@Directive({
  selector: '[lpValidatorPasswordEqualled]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: ValidatorPasswordEqualledDirective,
      multi: true
    }
  ]
})
export class ValidatorPasswordEqualledDirective implements Validator {
  validate(control: AbstractControl): ValidationErrors {
    return ValidatorPasswordEqualled(control);
  }
}

const ValidatorPasswordEqualled: ValidatorFn = (
  control: FormGroup
): ValidationErrors | null => {
  const password = control.get(nameof(() => this.password));
  const reenterPassword = control.get(nameof(() => this.reenterPassword));

  const error =
    password && reenterPassword && password.value !== reenterPassword.value
      ? { passwordNotEqualled: true }
      : null;

  return error;
};
