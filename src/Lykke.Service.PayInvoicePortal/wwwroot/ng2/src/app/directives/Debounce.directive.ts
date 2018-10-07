import { Directive, Input, OnInit, ElementRef, OnDestroy } from '@angular/core';
import { fromEvent, Subscription, Observable } from 'rxjs';
import { map, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { NgModel } from '@angular/forms';

@Directive({
  selector: '[ngModel][lp-debounce]'
})
export class DebounceDirective implements OnInit, OnDestroy {
  @Input('lp-debounce')
  public debounceTime = 500;

  @Input('lp-debounce-onblur')
  private debounceOnBlur: boolean;

  private keyupSubscription: Subscription;
  private blurObservable: Observable<any>;
  private blurSubscription: Subscription;

  constructor(private elementRef: ElementRef, private model: NgModel) {}

  ngOnInit() {
    const keyupObservable = fromEvent(
      this.elementRef.nativeElement,
      'keyup'
    ).pipe(
      map(() => {
        return this.model.value;
      }),
      debounceTime(this.debounceTime),
      distinctUntilChanged()
    );

    if (this.debounceOnBlur) {
      this.blurObservable = fromEvent(
        this.elementRef.nativeElement,
        'blur'
      ).pipe(
        map(() => {
          return this.model.value;
        })
      );
    }

    this.model.viewToModelUpdate = () => {};

    this.keyupSubscription = keyupObservable.subscribe(value => {
      this.model.viewModel = value;
      this.model.update.emit(value);
    });

    if (this.debounceOnBlur) {
      this.blurSubscription = this.blurObservable.subscribe(value => {
        this.model.viewModel = value;
        this.model.update.emit(value);
      });
    }
  }

  ngOnDestroy() {
    this.keyupSubscription.unsubscribe();
    this.blurSubscription.unsubscribe();
  }
}
