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

@Directive({
  selector: '[lpValidatorOldPasswordNotEqualled]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: ValidatorOldPasswordNotEqualledDirective,
      multi: true
    }
  ]
})
export class ValidatorOldPasswordNotEqualledDirective implements Validator {
  validate(control: AbstractControl): ValidationErrors {
    return ValidatorOldPasswordNotEqualled(control);
  }
}

const ValidatorOldPasswordNotEqualled: ValidatorFn = (
  control: FormGroup
): ValidationErrors | null => {
  const currentPassword = control.get('currentPassword');
  const password = control.get('password');

  const error =
    currentPassword && password && currentPassword.value === password.value
      ? { passwordEqualled: true }
      : null;

  return error;
};
