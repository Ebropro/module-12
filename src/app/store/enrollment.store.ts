import { computed, inject } from '@angular/core';
import {
  signalStore,
  withComputed,
  withMethods,
  patchState,
  withState,
} from '@ngrx/signals';
import {
  withEntities,
  setAllEntities,
  updateEntity,
} from '@ngrx/signals/entities';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, concatMap, switchMap, tap, catchError, EMPTY } from 'rxjs';
import { EnrollmentService } from '../services/enrollment.service';
import { LiveSyncService } from '../services/live-sync.service';
import { Enrollment } from '../models/enrollment.model';

export const EnrollmentStore = signalStore(
  { providedIn: 'root' },

  withState({ isLoading: false, error: null as string | null }),

  withEntities<Enrollment>(),

  withComputed((store) => ({
    pendingCount: computed(
      () => store.entities().filter(e => e.status === 'Pending').length
    ),
  })),

withMethods((
  store,
  api = inject(EnrollmentService),
  sync = inject(LiveSyncService),
) => ({
  seed(rows: Enrollment[]) {
    patchState(store, setAllEntities(rows));
  },
    loadEnrollments: rxMethod<void>(
      pipe(
        tap(() => patchState(store, { isLoading: true, error: null })),
        concatMap(() =>
          api.getAll().pipe(
            tap(rows => patchState(store, setAllEntities(rows), { isLoading: false })),
            catchError(err => {
              patchState(store, { isLoading: false, error: err.message });
              return EMPTY;
            })
          )
        )
      )
    ),
    // NOTE: id is `number` here, not `string` — corrected to match the real 
    // backend's int Enrollment.Id (see enrollment.model.ts for explanation).
    approveEnrollment: rxMethod<number>(
      pipe(
        tap(id => {
          patchState(store, updateEntity({ id, changes: { status: 'Approved' } }));
        }),
        concatMap(id =>
          api.approve(id).pipe(
            catchError(err => {
              patchState(store, updateEntity({ id, changes: { status: 'Pending' } }));
              patchState(store, { error: 'Server rejected the approval. Check enrollment constraints.' });
              return EMPTY;
            })
          )
        )
      )
    ),

    // M9 -3: bridges SignalR push events into store state.
    // Note this uses `id` as a NUMBER internally (updateEntity matches 
    // real int-based Enrollment.id), even though SignalR transmits it as a
    // string over the wire — converted here at the boundary.
    listenForLiveUpdates: rxMethod<void>(
      pipe(
        tap(() => sync.connect()),
        switchMap(() => sync.events$),
        tap(event => {
          patchState(
            store,
            updateEntity({ id: Number(event.id), changes: { status: event.status } })
          );
        })
      )
    ),
  }))
);
