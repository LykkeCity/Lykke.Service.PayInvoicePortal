import {
  Component,
  Input,
  OnInit,
  ViewChild,
  AfterViewInit,
  ElementRef,
  forwardRef,
  OnChanges,
  SimpleChanges,
  Output,
  EventEmitter,
  AfterViewChecked
} from '@angular/core';
import { SelectPickerModel } from './SelectPickerModel';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { nameof } from '../../utils/utils';
declare const $: any;

@Component({
  selector: SelectPickerComponent.Selector,
  templateUrl: './SelectPicker.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SelectPickerComponent),
      multi: true
    }
  ]
})
export class SelectPickerComponent
  implements AfterViewInit, ControlValueAccessor, OnChanges, AfterViewChecked {
  static readonly Selector = 'lp-selectpicker';
  @Input()
  cssClasses?: string;
  @Input()
  label?: string;
  @Input()
  placeholder?: string;
  @Input()
  disabled?: boolean;
  @Input()
  revertPreviousItemTrigger?: number;
  selectedItem: string;
  @Input()
  selectpickerOptions: SelectPickerModel[] = [];
  @ViewChild('selectpicker')
  selectpickerElement: ElementRef;
  private previousItem: string;
  private needRefresh: boolean;
  private isInitialized: boolean;

  ngAfterViewInit() {
    setTimeout(() => {
      $(this.selectpickerElement.nativeElement).selectpicker({
        mobile: window['isMobile']
      });
      this.isInitialized = true;
    });
  }

  // ngModelChange should be before ngModel in markup to have oldValue
  onChangedSelectedItem(newValue) {
    this.previousItem = this.selectedItem;
    this.selectedItem = newValue;
    this.onChange(this.selectedItem);
  }

  // #region NgModel
  // the ngModel value has been set in code
  writeValue(value: any): void {
    this.onChangedSelectedItem(value);

    if (this.isInitialized && (value !== undefined || value !== null)) {
      this.refresh();
    }
  }
  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  onChange: (_: any) => void = (_: any) => {};
  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }
  onTouched: () => void = () => {};
  setDisabledState?(isDisabled: boolean): void {}
  // #endregion NgModel
  ngOnChanges(changes: SimpleChanges): void {
    if (
      changes[nameof(() => this.revertPreviousItemTrigger)] &&
      !changes[nameof(() => this.revertPreviousItemTrigger)].firstChange
    ) {
      const temp = this.previousItem;
      this.previousItem = this.selectedItem;
      this.selectedItem = temp;
      this.needRefresh = true;
      this.onChange(this.selectedItem);
    }

    if (
      changes[nameof(() => this.selectpickerOptions)] &&
      !changes[nameof(() => this.selectpickerOptions)].firstChange
    ) {
      this.needRefresh = true;
    }
  }

  ngAfterViewChecked(): void {
    if (this.needRefresh) {
      this.needRefresh = false;
      this.refresh();
    }
  }

  private refresh(): void {
    setTimeout(() => {
      $(this.selectpickerElement.nativeElement).selectpicker('refresh');
    });
  }
}
