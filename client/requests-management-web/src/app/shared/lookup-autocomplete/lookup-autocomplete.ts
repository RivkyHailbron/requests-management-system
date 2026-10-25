import { ChangeDetectionStrategy, Component, forwardRef, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-lookup-autocomplete',
  imports: [ReactiveFormsModule, MatAutocompleteModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule],
  templateUrl: './lookup-autocomplete.html',
  styleUrl: './lookup-autocomplete.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => LookupAutocomplete), multi: true }],
})
export class LookupAutocomplete implements ControlValueAccessor {
  readonly label = input.required<string>();
  readonly options = input.required<readonly string[]>();
  readonly searchChange = output<string>();

  protected readonly text = new FormControl<string | null>('');
  protected readonly hasText = signal(false);

  private selected = '';
  private onChange: (value: string) => void = () => undefined;
  protected onTouched: () => void = () => undefined;

  constructor() {
    this.text.valueChanges.pipe(takeUntilDestroyed()).subscribe((value) => {
      const text = value ?? '';
      this.hasText.set(text.length > 0);
      this.searchChange.emit(text);
      if (!text && this.selected) {
        this.commit('');
      }
    });
  }

  writeValue(value: string | null): void {
    this.selected = value ?? '';
    this.text.setValue(this.selected, { emitEvent: false });
    this.hasText.set(this.selected.length > 0);
  }

  registerOnChange(onChange: (value: string) => void): void {
    this.onChange = onChange;
  }

  registerOnTouched(onTouched: () => void): void {
    this.onTouched = onTouched;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.text.disable({ emitEvent: false });
    } else {
      this.text.enable({ emitEvent: false });
    }
  }

  protected loadOptions(): void {
    this.searchChange.emit(this.text.value ?? '');
  }

  protected select(value: string): void {
    this.commit(value);
  }

  protected clear(): void {
    this.text.setValue('');
  }

  private commit(value: string): void {
    this.selected = value;
    this.onChange(value);
  }
}
