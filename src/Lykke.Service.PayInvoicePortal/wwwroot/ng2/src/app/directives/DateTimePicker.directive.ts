import {
  Directive,
  ElementRef,
  OnInit,
  NgZone,
  SimpleChanges
} from '@angular/core';
import { NgModel } from '@angular/forms';

declare const $: any;
declare const moment: any;

@Directive({
  selector: '[lpDateTimePicker]'
})
export class DateTimePickerDirective implements OnInit {
  private readonly minDate = moment().startOf('day');

  // https://eonasdan.github.io/bootstrap-datetimepicker/Options/
  private readonly datetimepickerOptions = {
    format: 'l',
    minDate: this.minDate,
    allowInputToggle: true,
    icons: {
      time: 'icon--clock',
      date: 'icon--cal',
      up: 'icon--chevron-thin-up',
      down: 'icon--chevron-thin-down',
      previous: 'icon--chevron-thin-left',
      next: 'icon--chevron-thin-right'
    }
  };

  constructor(
    private elementRef: ElementRef,
    private ngModel: NgModel,
    private zone: NgZone
  ) {}

  ngOnInit() {
    const dateTimePickerElement = $(this.elementRef.nativeElement).parent();

    dateTimePickerElement.datetimepicker(this.datetimepickerOptions)

    // https://eonasdan.github.io/bootstrap-datetimepicker/Events/
    dateTimePickerElement
      .on('dp.error', e => {
        const value = dateTimePickerElement.data('DateTimePicker').date();
        this.zone.run(() => {
            this.ngModel.update.emit(value);
        });
      });

    dateTimePickerElement
      .on('dp.change', e => {
        const value = dateTimePickerElement.data('DateTimePicker').date();

        this.zone.run(() => {
          if (
            (value && !this.ngModel.value) ||
            (!value && this.ngModel.value) ||
            (this.ngModel.value &&
              value &&
              this.ngModel.value.valueOf() !== value.valueOf())
          ) {
            this.ngModel.update.emit(value);
          }
        });
      });

    this.ngModel.valueChanges.subscribe(newValue => {
      if (newValue && typeof newValue !== 'string') {
        dateTimePickerElement.data('DateTimePicker').date(newValue.toDate());
      } else if (!newValue) {
        dateTimePickerElement.data('DateTimePicker').date(null);
      }
    });
  }
}
