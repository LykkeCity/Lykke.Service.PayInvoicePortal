// https://angular.io/guide/form-validation#custom-validators

import { Directive, Input } from '@angular/core';
import {
  NG_VALIDATORS,
  Validator,
  AbstractControl,
  ValidationErrors,
  ValidatorFn
} from '@angular/forms';

@Directive({
  selector: '[lpMinNumberValidator]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: MinNumberValidatorDirective,
      multi: true
    }
  ]
})
export class MinNumberValidatorDirective implements Validator {
  @Input('min')
  minValue: number;
  validate(control: AbstractControl): ValidationErrors {
    return minValueValidator(this.minValue)(control);
  }
}

function minValueValidator(minValue: number): ValidatorFn {
  return  (control: AbstractControl): ValidationErrors | null => {
    const num = +control.value;
    return (isNaN(num) || num < minValue) ? { min: true } : null;
  };
}
